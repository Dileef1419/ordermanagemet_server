using System.Data;
using Dapper;
using Payments.Application.DTOs;
using Payments.Application.Queries.GetPaymentsByCustomer;

namespace Payments.Infrastructure.QueryHandlers;

public class GetPaymentsByCustomerQueryHandler : IGetPaymentsByCustomerQueryHandler
{
    private readonly IDbConnection _connection;

    public GetPaymentsByCustomerQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<IReadOnlyList<PaymentSummaryResponse>> Handle(
        GetPaymentsByCustomerQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                Id AS PaymentId,
                OrderId,
                Amount,
                Currency,
                Status,
                CreatedAt,
                LastUpdatedAt
            FROM pay.Payments
            WHERE (@CustomerId IS NULL OR CustomerId = @CustomerId)
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var results = await _connection.QueryAsync<PaymentSummaryResponse>(
            new CommandDefinition(sql, new
            {
                query.CustomerId,
                Offset = (query.Page - 1) * query.PageSize,
                query.PageSize
            }, cancellationToken: ct));

        return results.ToList();
    }
}
