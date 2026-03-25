using Payments.Application.DTOs;
using SharedKernel;

namespace Payments.Application.Queries.GetRevenue;

public interface IGetRevenueQueryHandler
    : IQueryHandler<GetRevenueQuery, IReadOnlyList<RevenueReportResponse>> { }
