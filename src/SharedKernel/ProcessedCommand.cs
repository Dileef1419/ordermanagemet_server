namespace SharedKernel;

public class ProcessedCommand
{
    public Guid IdempotencyKey { get; set; }
    public string CommandType { get; set; } = null!;
    public string? ResultPayload { get; set; }
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
}
