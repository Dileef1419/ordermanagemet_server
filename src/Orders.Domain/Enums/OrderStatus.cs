namespace Orders.Domain.Enums;

public enum OrderStatus
{
    Placed,
    Confirmed,
    Shipped,
    Delivered,
    Returned,
    Cancelled,
    Failed
}
