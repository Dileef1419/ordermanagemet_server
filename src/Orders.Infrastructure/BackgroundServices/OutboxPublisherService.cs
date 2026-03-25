using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.BackgroundServices;

/// <summary>
/// Background publisher polls the Outbox table for unpublished events
/// and publishes them to the message broker. In production, this would
/// publish to Azure Service Bus. Here it simulates the pattern.
/// </summary>
public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox publisher started.");

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var unpublished = db.Outbox
                    .Where(o => o.PublishedAt == null)
                    .OrderBy(o => o.CreatedAt)
                    .Take(50)
                    .ToList();

                foreach (var message in unpublished)
                {
                    // In production: await _serviceBus.PublishAsync(message);
                    _logger.LogInformation(
                        "Published outbox event {EventType} for aggregate {AggregateId}",
                        message.EventType, message.AggregateId);

                    message.PublishedAt = DateTimeOffset.UtcNow;
                }

                if (unpublished.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Outbox publisher error.");
            }
        }
    }

    public override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }
}
