namespace Orders.Domain.Exceptions;

public class InvalidOrderStateException : Exception
{
    public InvalidOrderStateException(Guid orderId, string currentStatus, string targetStatus)
        : base($"Order {orderId}: cannot transition from '{currentStatus}' to '{targetStatus}'.")
    {
    }
}

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid orderId)
        : base($"Order {orderId} not found.")
    {
    }
}
