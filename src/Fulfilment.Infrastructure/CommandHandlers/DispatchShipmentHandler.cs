using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Fulfilment.Application.Commands.DispatchShipment;
using Fulfilment.Application.DTOs;
using Fulfilment.Infrastructure.Persistence;
using SharedKernel;

namespace Fulfilment.Infrastructure.CommandHandlers;

public class DispatchShipmentHandler : IDispatchShipmentCommandHandler
{
    private readonly FulfilmentDbContext _db;
    public DispatchShipmentHandler(FulfilmentDbContext db) => _db = db;

    public async Task<ShipmentResponse> Handle(DispatchShipmentCommand cmd, CancellationToken ct)
    {
        var shipment = await _db.Shipments
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == cmd.ShipmentId, ct)
            ?? throw new InvalidOperationException($"Shipment {cmd.ShipmentId} not found.");

        shipment.Dispatch(cmd.CarrierRef, cmd.TrackingNumber);

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
