using Payments.Application.DTOs;
using SharedKernel;

namespace Payments.Application.Queries.GetPaymentByOrder;

public interface IGetPaymentByOrderQueryHandler
    : IQueryHandler<GetPaymentByOrderQuery, PaymentSummaryResponse?> { }
