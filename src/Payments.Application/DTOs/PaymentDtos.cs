namespace Payments.Application.DTOs;

// ── Request DTOs ──
public record AuthorisePaymentRequest(Guid OrderId, decimal Amount, string Currency);
public record RefundPaymentRequest(decimal Amount, string Reason);

// ── Response DTOs ──
public record PaymentResponse(Guid PaymentId, string Status);

public record PaymentSummaryResponse(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdatedAt);

public record RevenueReportResponse(
    DateOnly RevenueDate,
    string Currency,
    decimal TotalCaptured,
    decimal TotalRefunded,
    decimal NetRevenue,
    int TransactionCount);
