using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Commands.AuthorisePayment;
using Payments.Application.Commands.CapturePayment;
using Payments.Application.Commands.RefundPayment;
using Payments.Application.DTOs;
using Payments.Application.Queries.GetPaymentByOrder;
using Payments.Application.Queries.GetRevenue;

namespace Payments.Api.Controllers;

/// <summary>
/// Payments Controller — Authorise, Capture, Refund commands + read queries.
/// </summary>
[ApiController]
[Route("api/v1/payments")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    // ═══════════ COMMANDS (Write Model — EF Core) ═══════════

    /// <summary>Authorise a payment (idempotent).</summary>
    [HttpPost("authorise")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authorise(
        [FromBody] AuthorisePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromServices] IAuthorisePaymentCommandHandler handler,
        [FromServices] IValidator<AuthorisePaymentCommand> validator,
        CancellationToken ct)
    {
        if (idempotencyKey == Guid.Empty)
            return BadRequest("Idempotency-Key header required.");

        var command = new AuthorisePaymentCommand(idempotencyKey, request.OrderId, request.Amount, request.Currency);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await handler.Handle(command, ct);
        return Ok(result);
    }

    /// <summary>Capture a previously authorised payment.</summary>
    [HttpPost("{paymentId:guid}/capture")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Capture(
        Guid paymentId,
        [FromServices] ICapturePaymentCommandHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CapturePaymentCommand(paymentId), ct);
        return Ok(result);
    }

    /// <summary>Refund a captured payment.</summary>
    [HttpPost("{paymentId:guid}/refund")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Refund(
        Guid paymentId,
        [FromBody] RefundPaymentRequest request,
        [FromServices] IRefundPaymentCommandHandler handler,
        [FromServices] IValidator<RefundPaymentCommand> validator,
        CancellationToken ct)
    {
        var command = new RefundPaymentCommand(paymentId, request.Amount, request.Reason);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await handler.Handle(command, ct);
        return Ok(result);
    }

    // ═══════════ QUERIES (Read Model — Dapper) ═══════════

    /// <summary>Get payment by order ID.</summary>
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(typeof(PaymentSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrder(
        Guid orderId,
        [FromServices] IGetPaymentByOrderQueryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetPaymentByOrderQuery(orderId), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>Get daily revenue report.</summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(IReadOnlyList<RevenueReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] string? currency,
        [FromServices] IGetRevenueQueryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetRevenueQuery(from, to, currency), ct);
        return Ok(result);
    }
}
