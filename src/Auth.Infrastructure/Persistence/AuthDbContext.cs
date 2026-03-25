using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using SharedKernel;

namespace Auth.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<ProcessedCommand> ProcessedCommands => Set<ProcessedCommand>();

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
