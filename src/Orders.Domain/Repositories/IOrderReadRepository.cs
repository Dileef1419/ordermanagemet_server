namespace Orders.Domain.Repositories;

public interface IOrderReadRepository
{
    Task<OrderSummaryDto?> GetByIdAsync(Guid orderId, CancellationToken ct);
    Task<IReadOnlyList<OrderSummaryDto>> GetByCustomerAsync(
        Guid customerId, string? status, int page, int pageSize, CancellationToken ct);
    Task<OrderDashboardDto> GetDashboardAsync(CancellationToken ct);
}

// DTOs for read model
public record OrderSummaryDto(
    Guid OrderId,
    string CustomerName,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTimeOffset PlacedAt,
    DateTimeOffset LastUpdatedAt);

public record OrderDashboardDto(
    int Placed,
    int Confirmed,
    int Cancelled,
    int Failed);
