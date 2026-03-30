namespace Orders.Application.Commands.ReturnOrder;

public record ReturnOrderCommand(Guid OrderId, string Reason);
