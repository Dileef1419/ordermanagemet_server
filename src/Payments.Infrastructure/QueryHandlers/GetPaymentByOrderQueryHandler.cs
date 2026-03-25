using System.Data;
using Dapper;
using Payments.Application.DTOs;
using Payments.Application.Queries.GetPaymentByOrder;

namespace Payments.Infrastructure.QueryHandlers;

public class GetPaymentByOrderQueryHandler : IGetPaymentByOrderQueryHandler
{
    private readonly IDbConnection _connection;
    public GetPaymentByOrderQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<PaymentSummaryResponse?> Handle(GetPaymentByOrderQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                Id AS PaymentId, OrderId, Amount, Currency,
                Status, CreatedAt, LastUpdatedAt
            FROM pay.Payments
            WHERE OrderId = @OrderId;";

        return await _connection.QuerySingleOrDefaultAsync<PaymentSummaryResponse>(
            new CommandDefinition(sql, new { query.OrderId }, cancellationToken: ct));
    }
}
