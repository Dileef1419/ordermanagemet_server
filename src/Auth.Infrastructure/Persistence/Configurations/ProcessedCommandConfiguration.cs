using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Auth.Infrastructure.Persistence.Configurations;

public class ProcessedCommandConfiguration : IEntityTypeConfiguration<ProcessedCommand>
{
    public void Configure(EntityTypeBuilder<ProcessedCommand> builder)
    {
        builder.ToTable("ProcessedCommands");
        builder.HasKey(p => p.IdempotencyKey);
        builder.Property(p => p.CommandType).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ResultPayload).HasColumnType("nvarchar(max)");
        builder.Property(p => p.ProcessedAt).IsRequired();
        builder.Property(p => p.ExpiresAt).IsRequired();
    }
}
