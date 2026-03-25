using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Orders.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("Outbox");
        builder.HasKey(o => o.EventId);
        builder.Property(o => o.EventType).HasMaxLength(200);
        builder.HasIndex(o => o.PublishedAt)
            .HasFilter("[PublishedAt] IS NULL");
    }
}

public class ProcessedCommandConfiguration : IEntityTypeConfiguration<ProcessedCommand>
{
    public void Configure(EntityTypeBuilder<ProcessedCommand> builder)
    {
        builder.ToTable("ProcessedCommands");
        builder.HasKey(p => p.IdempotencyKey);
        builder.Property(p => p.CommandType).HasMaxLength(200);
    }
}
