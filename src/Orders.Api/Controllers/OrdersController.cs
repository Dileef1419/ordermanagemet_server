using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Commands.CancelOrder;
using Orders.Application.Commands.PlaceOrder;
using Orders.Application.DTOs;
using Orders.Application.Queries.GetDashboard;
using Orders.Application.Queries.GetOrderById;
using Orders.Application.Queries.GetOrdersByCustomer;

namespace Orders.Api.Controllers;

/// <summary>
/// Orders Controller — separates Command (write via EF Core) and Query (read via Dapper) endpoints.
/// Follows CQRS pattern: POST/PUT = Commands, GET = Queries.
/// </summary>
[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    // ═══════════ COMMANDS (Write Model — EF Core) ══════════=

    /// <summary>Place a new order (idempotent — requires Idempotency-Key header).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromServices] IPlaceOrderCommandHandler handler,
        [FromServices] IValidator<PlaceOrderCommand> validator,
        CancellationToken ct)
    {
        if (idempotencyKey == Guid.Empty)
            return BadRequest("Idempotency-Key header is required (GUID).");

        var command = new PlaceOrderCommand(
            idempotencyKey,
            request.CustomerId,
            request.CustomerName,
            request.Lines.Select(l => new OrderLineItemCommand(l.Sku, l.Quantity, l.UnitPrice)).ToList());

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await handler.Handle(command, ct);
        return CreatedAtAction(nameof(GetOrderById), new { orderId = result.OrderId }, result);
    }

    /// <summary>Cancel an existing order.</summary>
    [HttpPut("{orderId:guid}/cancel")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(
        Guid orderId,
        [FromBody] CancelOrderRequest request,
        [FromServices] ICancelOrderCommandHandler handler,
        CancellationToken ct)
    {
        var command = new CancelOrderCommand(orderId, request.Reason);
        var result = await handler.Handle(command, ct);
        return Ok(result);
    }

    // ═══════════ QUERIES (Read Model — Dapper) ═══════════

    /// <summary>Get order by ID (Dapper read model).</summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(
        Guid orderId,
        [FromServices] IGetOrderByIdQueryHandler handler,
        CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(orderId);
        var result = await handler.Handle(query, ct);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>Get orders by customer (paginated, filterable by status).</summary>
    [HttpGet("by-customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByCustomer(
        Guid customerId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] IGetOrdersByCustomerQueryHandler? handler = null,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = new GetOrdersByCustomerQuery(customerId, status, page, pageSize);
        var result = await handler!.Handle(query, ct);
        return Ok(result);
    }

    /// <summary>Get order dashboard (aggregate counts by status).</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(OrderDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromServices] IGetDashboardQueryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetDashboardQuery(), ct);
        return Ok(result);
    }
}
