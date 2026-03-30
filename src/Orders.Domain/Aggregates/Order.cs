using Orders.Domain.Enums;
using Orders.Domain.Events;
using Orders.Domain.Exceptions;
using SharedKernel;

namespace Orders.Domain.Aggregates;

public sealed class Order : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "AUD";
    public DateTimeOffset PlacedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;
    
    public Address? ShippingAddress { get; private set; }

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { } // EF Core

    public static Order Place(Guid customerId, string customerName, Address address, IReadOnlyList<OrderLineInput> lines)
    {
        if (lines.Count == 0)
            throw new ArgumentException("Order must have at least one line.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerName = customerName,
            ShippingAddress = address,
            Status = OrderStatus.Placed,
            Currency = "AUD",
            PlacedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        foreach (var input in lines)
            order._lines.Add(new OrderLine(order.Id, input.Sku, input.Quantity, input.UnitPrice));

        order.TotalAmount = order._lines.Sum(l => l.Quantity * l.UnitPrice);

        order.RaiseDomainEvent(new OrderPlacedEvent(
            order.Id, order.CustomerId, order.TotalAmount, order.Currency));

        return order;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Placed)
            throw new InvalidOrderStateException(Id, Status.ToString(), nameof(OrderStatus.Confirmed));

        Status = OrderStatus.Confirmed;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderConfirmedEvent(Id));
    }

    public void Cancel(string reason)
    {
        if (Status is not (OrderStatus.Placed or OrderStatus.Confirmed))
            throw new InvalidOrderStateException(Id, Status.ToString(), nameof(OrderStatus.Cancelled));

        Status = OrderStatus.Cancelled;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void MarkFailed(string failureReason)
    {
        if (Status != OrderStatus.Placed)
            throw new InvalidOrderStateException(Id, Status.ToString(), nameof(OrderStatus.Failed));

        Status = OrderStatus.Failed;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderFailedEvent(Id, failureReason));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOrderStateException(Id, Status.ToString(), nameof(OrderStatus.Shipped));

        Status = OrderStatus.Shipped;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderShippedEvent(Id));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOrderStateException(Id, Status.ToString(), nameof(OrderStatus.Delivered));

        Status = OrderStatus.Delivered;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderDeliveredEvent(Id));
    }

    public void Return(string reason)
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidOrderStateException(Id, Status.ToString(), nameof(OrderStatus.Returned));

        Status = OrderStatus.Returned;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderReturnedEvent(Id, reason));
    }
}

public sealed class OrderLine
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Sku { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderLine() { } // EF Core

    internal OrderLine(Guid orderId, string sku, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

public record OrderLineInput(string Sku, int Quantity, decimal UnitPrice);

public record Address(string FullName, string AddressLine1, string City, string PostalCode, string Country);
