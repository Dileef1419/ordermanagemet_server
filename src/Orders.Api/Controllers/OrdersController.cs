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
        try
        {
            if (idempotencyKey == Guid.Empty)
                return BadRequest("Idempotency-Key header is required (GUID).");

            if (request.ShippingAddress is null)
                return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> { { "ShippingAddress", new[] { "Shipping Address is required." } } }));

            var address = new Domain.Aggregates.Address(
                request.ShippingAddress.FullName,
                request.ShippingAddress.AddressLine1,
                request.ShippingAddress.City,
                request.ShippingAddress.PostalCode,
                request.ShippingAddress.Country);

            var command = new PlaceOrderCommand(
                idempotencyKey,
                request.CustomerId,
                request.CustomerName,
                address,
                request.Lines.Select(l => new OrderLineItemCommand(l.Sku, l.Quantity, l.UnitPrice)).ToList());

            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return BadRequest(new ValidationProblemDetails(
                    validation.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

            var result = await handler.Handle(command, ct);
            return CreatedAtAction(nameof(GetOrderById), new { orderId = result.OrderId }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
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
        try
        {
            var command = new CancelOrderCommand(orderId, request.Reason);
            var result = await handler.Handle(command, ct);
            return Ok(result);
        }
        catch (Orders.Domain.Exceptions.OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Orders.Domain.Exceptions.InvalidOrderStateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Confirm an order after payment.</summary>
    [HttpPut("{orderId:guid}/confirm")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmOrder(
        Guid orderId,
        [FromServices] Orders.Application.Commands.ConfirmOrder.IConfirmOrderCommandHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new Orders.Application.Commands.ConfirmOrder.ConfirmOrderCommand(orderId), ct);
        return Ok(result);
    }

    /// <summary>Mark order as failed after payment failure.</summary>
    [HttpPut("{orderId:guid}/fail")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> FailOrder(
        Guid orderId,
        [FromBody] FailOrderRequest request,
        [FromServices] Orders.Application.Commands.MarkOrderFailed.IMarkOrderFailedCommandHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new Orders.Application.Commands.MarkOrderFailed.MarkOrderFailedCommand(orderId, request.Reason), ct);
        return Ok(result);
    }

    // ═══════════ QUERIES (Read Model — Dapper) ═══════════

    /// <summary>Get order by ID (Dapper read model).</summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(DetailedOrderResponse), StatusCodes.Status200OK)]
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

    /// <summary>Get all orders (Admin only).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] IGetOrdersByCustomerQueryHandler handler = null!,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = new GetOrdersByCustomerQuery(null, status, page, pageSize);
        var result = await handler.Handle(query, ct);
        return Ok(result);
    }

    /// <summary>Update order status (Admin only).</summary>
    [HttpPut("{orderId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        [FromServices] ICancelOrderCommandHandler cancelHandler,
        [FromServices] Orders.Application.Commands.ShipOrder.IShipOrderCommandHandler shipHandler,
        [FromServices] Orders.Application.Commands.DeliverOrder.IDeliverOrderCommandHandler deliverHandler,
        [FromServices] Orders.Application.Commands.ReturnOrder.IReturnOrderCommandHandler returnHandler,
        CancellationToken ct)
    {
        try
        {
            switch (request.Status)
            {
                case "Cancelled":
                    await cancelHandler.Handle(new CancelOrderCommand(orderId, "Admin override"), ct);
                    break;
                case "Shipped":
                    await shipHandler.Handle(new Orders.Application.Commands.ShipOrder.ShipOrderCommand(orderId), ct);
                    break;
                case "Delivered":
                    await deliverHandler.Handle(new Orders.Application.Commands.DeliverOrder.DeliverOrderCommand(orderId), ct);
                    break;
                case "Returned":
                    await returnHandler.Handle(new Orders.Application.Commands.ReturnOrder.ReturnOrderCommand(orderId, "Customer return"), ct);
                    break;
                default:
                    return BadRequest(new { message = $"Status transition to '{request.Status}' is not supported." });
            }
            
            return NoContent();
        }
        catch (Orders.Domain.Exceptions.OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Orders.Domain.Exceptions.InvalidOrderStateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An internal error occurred", details = ex.Message });
        }
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

public record UpdateOrderStatusRequest(string Status);
public record FailOrderRequest(string Reason);
