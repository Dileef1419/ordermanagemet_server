using System.Data;
using Dapper;
using Payments.Application.DTOs;
using Payments.Application.Queries.GetRevenue;

namespace Payments.Infrastructure.QueryHandlers;

public class GetRevenueQueryHandler : IGetRevenueQueryHandler
{
    private readonly IDbConnection _connection;
    public GetRevenueQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<IReadOnlyList<RevenueReportResponse>> Handle(
        GetRevenueQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                CAST(CreatedAt AS DATE) AS RevenueDate,
                Currency,
                SUM(CASE WHEN Status = 'Captured' THEN Amount ELSE 0 END) AS TotalCaptured,
                SUM(CASE WHEN Status = 'Refunded' THEN Amount ELSE 0 END) AS TotalRefunded,
                SUM(CASE WHEN Status = 'Captured' THEN Amount ELSE 0 END)
                  - SUM(CASE WHEN Status = 'Refunded' THEN Amount ELSE 0 END) AS NetRevenue,
                COUNT(*) AS TransactionCount
            FROM pay.Payments
            WHERE CAST(CreatedAt AS DATE) BETWEEN @From AND @To
              AND (@Currency IS NULL OR Currency = @Currency)
            GROUP BY CAST(CreatedAt AS DATE), Currency
            ORDER BY CAST(CreatedAt AS DATE) DESC;";

        var results = await _connection.QueryAsync<RevenueReportResponse>(
            new CommandDefinition(sql, new { query.From, query.To, query.Currency },
                cancellationToken: ct));
        return results.ToList();
    }
}
