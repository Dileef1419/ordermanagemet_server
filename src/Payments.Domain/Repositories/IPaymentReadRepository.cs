namespace Payments.Domain.Repositories;

public interface IPaymentReadRepository
{
    Task<PaymentSummaryDto?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);
    Task<IReadOnlyList<RevenueReportDto>> GetDailyRevenueAsync(
        DateOnly from, DateOnly to, string? currency, CancellationToken ct);
}

public record PaymentSummaryDto(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdatedAt);

public record RevenueReportDto(
    DateOnly RevenueDate,
    string Currency,
    decimal TotalCaptured,
    decimal TotalRefunded,
    decimal NetRevenue,
    int TransactionCount);
