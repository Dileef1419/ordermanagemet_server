using System.Data;
using Dapper;
using Orders.Application.DTOs;
using Orders.Application.Queries.GetOrderById;

namespace Orders.Infrastructure.QueryHandlers;

/// <summary>
/// Dapper-based read model handler — bypasses EF Core for performance.
/// </summary>
public class GetOrderByIdQueryHandler : IGetOrderByIdQueryHandler
{
    private readonly IDbConnection _connection;

    public GetOrderByIdQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<OrderSummaryResponse?> Handle(GetOrderByIdQuery query, CancellationToken ct)
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

        return await _connection.QuerySingleOrDefaultAsync<OrderSummaryResponse>(
            new CommandDefinition(sql, new { query.OrderId }, cancellationToken: ct));
    }
}
