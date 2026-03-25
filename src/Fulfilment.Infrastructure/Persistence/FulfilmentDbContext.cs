using Microsoft.EntityFrameworkCore;
using Fulfilment.Domain.Aggregates;
using SharedKernel;

namespace Fulfilment.Infrastructure.Persistence;

public class FulfilmentDbContext : DbContext
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<ProcessedCommand> ProcessedCommands => Set<ProcessedCommand>();

    public FulfilmentDbContext(DbContextOptions<FulfilmentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ful");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FulfilmentDbContext).Assembly);
    }
}
