namespace Orders.Application.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(
    Guid? CustomerId,
    string? Status,
    int Page,
    int PageSize);
