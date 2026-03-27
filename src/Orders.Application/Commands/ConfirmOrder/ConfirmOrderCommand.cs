namespace Orders.Application.Commands.ConfirmOrder;

/// <summary>
/// CQRS Command: confirms an order after successful payment.
/// </summary>
public record ConfirmOrderCommand(Guid OrderId);
