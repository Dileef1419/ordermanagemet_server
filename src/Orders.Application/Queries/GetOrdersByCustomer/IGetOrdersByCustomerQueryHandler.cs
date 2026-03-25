using Orders.Application.DTOs;
using SharedKernel;

namespace Orders.Application.Queries.GetOrdersByCustomer;

public interface IGetOrdersByCustomerQueryHandler
    : IQueryHandler<GetOrdersByCustomerQuery, IReadOnlyList<OrderSummaryResponse>> { }
