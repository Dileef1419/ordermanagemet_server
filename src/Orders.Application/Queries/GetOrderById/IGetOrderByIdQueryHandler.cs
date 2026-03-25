using Orders.Application.DTOs;
using SharedKernel;

namespace Orders.Application.Queries.GetOrderById;

public interface IGetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderSummaryResponse?> { }
