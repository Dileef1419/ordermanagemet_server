using Payments.Application.DTOs;

namespace Payments.Application.Queries.GetPaymentsByCustomer;

public record GetPaymentsByCustomerQuery(Guid? CustomerId, int Page = 1, int PageSize = 20);
