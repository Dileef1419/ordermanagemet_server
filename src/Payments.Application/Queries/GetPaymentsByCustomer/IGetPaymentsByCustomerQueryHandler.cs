using Payments.Application.DTOs;

namespace Payments.Application.Queries.GetPaymentsByCustomer;

public interface IGetPaymentsByCustomerQueryHandler
{
    Task<IReadOnlyList<PaymentSummaryResponse>> Handle(GetPaymentsByCustomerQuery query, CancellationToken ct);
}
