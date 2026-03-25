namespace Fulfilment.Application.Queries.GetWarehouseQueue;

public record GetWarehouseQueueQuery(
    int WarehouseId,
    string? Status,
    int Page,
    int PageSize);
