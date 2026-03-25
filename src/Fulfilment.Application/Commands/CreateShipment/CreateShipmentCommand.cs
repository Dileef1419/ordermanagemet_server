using Fulfilment.Domain.Aggregates;

namespace Fulfilment.Application.Commands.CreateShipment;

public record CreateShipmentCommand(
    Guid OrderId,
    int WarehouseId,
    IReadOnlyList<ShipmentItemInput> Items);
