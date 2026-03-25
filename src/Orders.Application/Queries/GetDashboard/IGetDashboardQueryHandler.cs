using Orders.Application.DTOs;
using SharedKernel;

namespace Orders.Application.Queries.GetDashboard;

public interface IGetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, OrderDashboardResponse> { }
