using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Fulfilment.Application.Commands.CreateShipment;
using Fulfilment.Application.Commands.DispatchShipment;
using Fulfilment.Application.DTOs;
using Fulfilment.Application.Queries.GetShipmentByOrder;
using Fulfilment.Application.Queries.GetWarehouseQueue;
using Fulfilment.Domain.Aggregates;

namespace Fulfilment.Api.Controllers;

/// <summary>
/// Fulfilment Controller — shipment creation, dispatch, and warehouse queue queries.
/// </summary>
[ApiController]
[Route("api/v1/fulfilment")]
[Produces("application/json")]
public class FulfilmentController : ControllerBase
{
    // ═══════════ COMMANDS (Write Model — EF Core) ═══════════

    /// <summary>Create a new shipment for an order.</summary>
    [HttpPost("shipments")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShipment(
        [FromBody] CreateShipmentRequest request,
        [FromServices] ICreateShipmentCommandHandler handler,
        [FromServices] IValidator<CreateShipmentCommand> validator,
        CancellationToken ct)
    {
        var items = request.Items.Select(i => new ShipmentItemInput(i.Sku, i.Quantity)).ToList();
        var command = new CreateShipmentCommand(request.OrderId, request.WarehouseId, items);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await handler.Handle(command, ct);
        return CreatedAtAction(nameof(GetByOrder), new { orderId = request.OrderId }, result);
    }

    /// <summary>Dispatch a shipment with carrier details.</summary>
    [HttpPut("shipments/{shipmentId:guid}/dispatch")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DispatchShipment(
        Guid shipmentId,
        [FromBody] DispatchShipmentRequest request,
        [FromServices] IDispatchShipmentCommandHandler handler,
        [FromServices] IValidator<DispatchShipmentCommand> validator,
        CancellationToken ct)
    {
        var command = new DispatchShipmentCommand(shipmentId, request.CarrierRef, request.TrackingNumber);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await handler.Handle(command, ct);
        return Ok(result);
    }

    // ═══════════ QUERIES (Read Model — Dapper) ═══════════

    /// <summary>Get shipment details by order ID.</summary>
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(typeof(ShipmentSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrder(
        Guid orderId,
        [FromServices] IGetShipmentByOrderQueryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetShipmentByOrderQuery(orderId), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>Get warehouse fulfilment queue (paginated).</summary>
    [HttpGet("warehouse/{warehouseId:int}/queue")]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseQueueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarehouseQueue(
        int warehouseId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] IGetWarehouseQueueQueryHandler? handler = null,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await handler!.Handle(new GetWarehouseQueueQuery(warehouseId, status, page, pageSize), ct);
        return Ok(result);
    }
}
