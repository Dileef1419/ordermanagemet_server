namespace Fulfilment.Domain.Repositories;

public interface IShipmentReadRepository
{
    Task<ShipmentSummaryDto?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);
    Task<IReadOnlyList<WarehouseQueueDto>> GetWarehouseQueueAsync(
        int warehouseId, string? status, int page, int pageSize, CancellationToken ct);
}

public record ShipmentSummaryDto(
    Guid ShipmentId,
    Guid OrderId,
    int WarehouseId,
    string Status,
    string? TrackingNumber,
    string? CarrierRef,
    DateTimeOffset CreatedAt);

public record WarehouseQueueDto(
    Guid ShipmentId,
    Guid OrderId,
    string Status,
    DateTimeOffset CreatedAt);
