using Fulfilment.Application.DTOs;
using SharedKernel;

namespace Fulfilment.Application.Queries.GetWarehouseQueue;

public interface IGetWarehouseQueueQueryHandler
    : IQueryHandler<GetWarehouseQueueQuery, IReadOnlyList<WarehouseQueueResponse>> { }
