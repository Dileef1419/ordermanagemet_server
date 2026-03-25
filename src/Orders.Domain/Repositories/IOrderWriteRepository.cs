using Orders.Domain.Aggregates;

namespace Orders.Domain.Repositories;

public interface IOrderWriteRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
