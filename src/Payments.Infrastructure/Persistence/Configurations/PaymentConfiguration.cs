using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Aggregates;

namespace Payments.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.RowVersion).IsRowVersion();
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(p => p.IdempotencyKey).IsUnique();
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.CustomerId);

        builder.Property(p => p.CustomerId).IsRequired();

        builder.HasMany(p => p.Attempts)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.DomainEvents);
    }
}

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("PaymentAttempts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(50);
        builder.Property(a => a.GatewayResponse).HasMaxLength(2000);
    }
}
