using System.Data;
using Dapper;
using Orders.Domain.Repositories;

namespace Orders.Infrastructure.Repositories;

/// <summary>
/// Read repository using Dapper for high-performance read model queries.
/// Queries hit the read-optimised projections, not the write model directly.
/// </summary>
public class OrderReadRepository : IOrderReadRepository
{
    private readonly IDbConnection _connection;

    public OrderReadRepository(IDbConnection connection) => _connection = connection;

    public async Task<OrderSummaryDto?> GetByIdAsync(Guid orderId, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                o.Id AS OrderId,
                o.CustomerName,
                o.CustomerId,
                o.Status,
                o.TotalAmount,
                o.Currency,
                (SELECT COUNT(*) FROM ord.OrderLines l WHERE l.OrderId = o.Id) AS ItemCount,
                o.PlacedAt,
                o.LastUpdatedAt
            FROM ord.Orders o
            WHERE o.Id = @OrderId;";

        return await _connection.QuerySingleOrDefaultAsync<OrderSummaryDto>(
            new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetByCustomerAsync(
        Guid customerId, string? status, int page, int pageSize, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                o.Id AS OrderId,
                o.CustomerName,
                o.CustomerId,
                o.Status,
                o.TotalAmount,
                o.Currency,
                (SELECT COUNT(*) FROM ord.OrderLines l WHERE l.OrderId = o.Id) AS ItemCount,
                o.PlacedAt,
                o.LastUpdatedAt
            FROM ord.Orders o
            WHERE o.CustomerId = @CustomerId
              AND (@Status IS NULL OR o.Status = @Status)
            ORDER BY o.PlacedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var results = await _connection.QueryAsync<OrderSummaryDto>(
            new CommandDefinition(sql, new
            {
                CustomerId = customerId,
                Status = status,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            }, cancellationToken: ct));

        return results.ToList();
    }

    public async Task<OrderDashboardDto> GetDashboardAsync(CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                SUM(CASE WHEN Status = 'Placed' THEN 1 ELSE 0 END) AS Placed,
                SUM(CASE WHEN Status = 'Confirmed' THEN 1 ELSE 0 END) AS Confirmed,
                SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END) AS Cancelled,
                SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS Failed
            FROM ord.Orders;";

        return await _connection.QuerySingleAsync<OrderDashboardDto>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
