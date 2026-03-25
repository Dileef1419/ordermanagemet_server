using System.Data;
using Dapper;
using Orders.Application.DTOs;
using Orders.Application.Queries.GetOrdersByCustomer;

namespace Orders.Infrastructure.QueryHandlers;

public class GetOrdersByCustomerQueryHandler : IGetOrdersByCustomerQueryHandler
{
    private readonly IDbConnection _connection;

    public GetOrdersByCustomerQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<IReadOnlyList<OrderSummaryResponse>> Handle(
        GetOrdersByCustomerQuery query, CancellationToken ct)
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

        var results = await _connection.QueryAsync<OrderSummaryResponse>(
            new CommandDefinition(sql, new
            {
                query.CustomerId,
                query.Status,
                Offset = (query.Page - 1) * query.PageSize,
                query.PageSize
            }, cancellationToken: ct));

        return results.ToList();
    }
}
