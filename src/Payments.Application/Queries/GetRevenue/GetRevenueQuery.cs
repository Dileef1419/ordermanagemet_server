namespace Payments.Application.Queries.GetRevenue;

public record GetRevenueQuery(DateOnly From, DateOnly To, string? Currency);
