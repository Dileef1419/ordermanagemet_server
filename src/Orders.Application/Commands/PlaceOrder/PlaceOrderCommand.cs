using Orders.Application.DTOs;

namespace Orders.Application.Commands.PlaceOrder;

/// <summary>
/// CQRS Command: creates a new order via the write model (EF Core).
/// Idempotency guaranteed via IdempotencyKey.
/// </summary>
public record PlaceOrderCommand(
    Guid IdempotencyKey,
    Guid CustomerId,
    string CustomerName,
    IReadOnlyList<OrderLineItemCommand> Lines);

public record OrderLineItemCommand(string Sku, int Quantity, decimal UnitPrice);
