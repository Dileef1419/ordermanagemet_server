namespace Orders.Application.Commands.CancelOrder;

/// <summary>
/// CQRS Command: cancels an existing order.
/// </summary>
public record CancelOrderCommand(Guid OrderId, string Reason);
