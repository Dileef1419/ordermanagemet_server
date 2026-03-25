using System.Data;
using Dapper;
using Orders.Application.DTOs;
using Orders.Application.Queries.GetDashboard;

namespace Orders.Infrastructure.QueryHandlers;

public class GetDashboardQueryHandler : IGetDashboardQueryHandler
{
    private readonly IDbConnection _connection;

    public GetDashboardQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<OrderDashboardResponse> Handle(GetDashboardQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                SUM(CASE WHEN Status = 'Placed' THEN 1 ELSE 0 END) AS Placed,
                SUM(CASE WHEN Status = 'Confirmed' THEN 1 ELSE 0 END) AS Confirmed,
                SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END) AS Cancelled,
                SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS Failed
            FROM ord.Orders;";

        return await _connection.QuerySingleAsync<OrderDashboardResponse>(
            new CommandDefinition(sql, cancellationToken: ct));
    }
}
