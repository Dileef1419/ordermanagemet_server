using System.Text.Json;
using Fulfilment.Application.Commands.CreateShipment;
using Fulfilment.Application.DTOs;
using Fulfilment.Domain.Aggregates;
using Fulfilment.Infrastructure.Persistence;
using SharedKernel;

namespace Fulfilment.Infrastructure.CommandHandlers;

public class CreateShipmentHandler : ICreateShipmentCommandHandler
{
    private readonly FulfilmentDbContext _db;
    public CreateShipmentHandler(FulfilmentDbContext db) => _db = db;

    public async Task<ShipmentResponse> Handle(CreateShipmentCommand cmd, CancellationToken ct)
    {
        var shipment = Shipment.Create(cmd.OrderId, cmd.WarehouseId, cmd.Items);
        await _db.Shipments.AddAsync(shipment, ct);

        foreach (var evt in shipment.DomainEvents)
        {
            _db.Outbox.Add(new OutboxMessage
            {
                AggregateId = shipment.Id,
                EventType = evt.GetType().Name,
                Payload = JsonSerializer.Serialize<object>(evt)
            });
        }

        await _db.SaveChangesAsync(ct);
        shipment.ClearDomainEvents();
        return new ShipmentResponse(shipment.Id, shipment.Status.ToString());
    }
}
