namespace Orders.Application.DTOs;

// ── Request DTOs (API → Application) ──
public record PlaceOrderRequest(
    Guid CustomerId,
    string CustomerName,
    List<OrderLineRequest> Lines);

public record OrderLineRequest(
    string Sku,
    int Quantity,
    decimal UnitPrice);

public record CancelOrderRequest(string Reason);

// ── Response DTOs (Application → API) ──
public record OrderResponse(Guid OrderId, string Status);

public record OrderSummaryResponse(
    Guid OrderId,
    string CustomerName,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTimeOffset PlacedAt,
    DateTimeOffset LastUpdatedAt);

public record OrderDashboardResponse(
    int Placed,
    int Confirmed,
    int Cancelled,
    int Failed);
