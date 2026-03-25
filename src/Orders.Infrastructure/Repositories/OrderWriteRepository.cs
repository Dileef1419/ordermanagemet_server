using Microsoft.EntityFrameworkCore;
using Orders.Domain.Aggregates;
using Orders.Domain.Repositories;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Repositories;

public class OrderWriteRepository : IOrderWriteRepository
{
    private readonly OrdersDbContext _db;

    public OrderWriteRepository(OrdersDbContext db) => _db = db;

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct)
        => await _db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

    public async Task AddAsync(Order order, CancellationToken ct)
        => await _db.Orders.AddAsync(order, ct);

    public async Task SaveChangesAsync(CancellationToken ct)
        => await _db.SaveChangesAsync(ct);
}
