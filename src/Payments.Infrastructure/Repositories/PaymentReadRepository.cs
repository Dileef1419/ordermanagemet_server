using System.Data;
using Dapper;
using Payments.Domain.Repositories;

namespace Payments.Infrastructure.Repositories;

public class PaymentReadRepository : IPaymentReadRepository
{
    private readonly IDbConnection _connection;
    public PaymentReadRepository(IDbConnection connection) => _connection = connection;

    public async Task<PaymentSummaryDto?> GetByOrderIdAsync(Guid orderId, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                Id AS PaymentId, OrderId, Amount, Currency,
                Status, CreatedAt, LastUpdatedAt
            FROM pay.Payments
            WHERE OrderId = @OrderId;";

        return await _connection.QuerySingleOrDefaultAsync<PaymentSummaryDto>(
            new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<RevenueReportDto>> GetDailyRevenueAsync(
        DateOnly from, DateOnly to, string? currency, CancellationToken ct)
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

        var results = await _connection.QueryAsync<RevenueReportDto>(
            new CommandDefinition(sql, new { From = from, To = to, Currency = currency },
                cancellationToken: ct));
        return results.ToList();
    }
}
