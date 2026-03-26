namespace SharedKernel;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }
}
