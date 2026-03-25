namespace Fulfilment.Application.DTOs;

// ── Request DTOs ──
public record CreateShipmentRequest(
    Guid OrderId,
    int WarehouseId,
    List<ShipmentItemRequest> Items);

public record ShipmentItemRequest(string Sku, int Quantity);
public record DispatchShipmentRequest(string CarrierRef, string TrackingNumber);

// ── Response DTOs ──
public record ShipmentResponse(Guid ShipmentId, string Status);

public record ShipmentSummaryResponse(
    Guid ShipmentId,
    Guid OrderId,
    int WarehouseId,
    string Status,
    string? TrackingNumber,
    string? CarrierRef,
    DateTimeOffset CreatedAt);

public record WarehouseQueueResponse(
    Guid ShipmentId,
    Guid OrderId,
    string Status,
    DateTimeOffset CreatedAt);
