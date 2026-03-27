namespace Orders.Application.Commands.MarkOrderFailed;

/// <summary>
/// CQRS Command: marks an order as failed after payment failure.
/// </summary>
public record MarkOrderFailedCommand(Guid OrderId, string Reason);
