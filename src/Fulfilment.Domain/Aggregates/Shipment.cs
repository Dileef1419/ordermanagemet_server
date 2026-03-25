using Fulfilment.Domain.Enums;
using Fulfilment.Domain.Events;
using SharedKernel;

namespace Fulfilment.Domain.Aggregates;

public sealed class Shipment : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public int WarehouseId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? CarrierRef { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;

    private readonly List<ShipmentItem> _items = new();
    public IReadOnlyCollection<ShipmentItem> Items => _items.AsReadOnly();

    private Shipment() { }

    public static Shipment Create(Guid orderId, int warehouseId, IReadOnlyList<ShipmentItemInput> items)
    {
        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            WarehouseId = warehouseId,
            Status = ShipmentStatus.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
        foreach (var item in items)
            shipment._items.Add(new ShipmentItem(shipment.Id, item.Sku, item.Quantity));

        shipment.RaiseDomainEvent(new ShipmentCreatedEvent(shipment.Id, orderId));
        return shipment;
    }

    public void Dispatch(string carrierRef, string trackingNumber)
    {
        if (Status != ShipmentStatus.Created)
            throw new InvalidOperationException($"Cannot dispatch shipment in '{Status}' state.");

        CarrierRef = carrierRef;
        TrackingNumber = trackingNumber;
        Status = ShipmentStatus.Dispatched;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ShipmentDispatchedEvent(Id, trackingNumber, carrierRef));
    }

    public void MarkDelivered()
    {
        if (Status != ShipmentStatus.Dispatched)
            throw new InvalidOperationException($"Cannot deliver shipment in '{Status}' state.");

        Status = ShipmentStatus.Delivered;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ShipmentDeliveredEvent(Id, DateTimeOffset.UtcNow));
    }

    public void RecordException(string exceptionType, string details)
    {
        Status = ShipmentStatus.Exception;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ShipmentExceptionEvent(Id, exceptionType, details));
    }
}

public sealed class ShipmentItem
{
    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public string Sku { get; private set; } = null!;
    public int Quantity { get; private set; }

    private ShipmentItem() { }

    internal ShipmentItem(Guid shipmentId, string sku, int quantity)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        Sku = sku;
        Quantity = quantity;
    }
}

public record ShipmentItemInput(string Sku, int Quantity);
