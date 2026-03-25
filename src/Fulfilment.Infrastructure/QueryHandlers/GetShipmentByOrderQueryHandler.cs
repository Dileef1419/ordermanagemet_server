using System.Data;
using Dapper;
using Fulfilment.Application.DTOs;
using Fulfilment.Application.Queries.GetShipmentByOrder;

namespace Fulfilment.Infrastructure.QueryHandlers;

public class GetShipmentByOrderQueryHandler : IGetShipmentByOrderQueryHandler
{
    private readonly IDbConnection _connection;
    public GetShipmentByOrderQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<ShipmentSummaryResponse?> Handle(GetShipmentByOrderQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id AS ShipmentId, OrderId, WarehouseId, Status,
                   TrackingNumber, CarrierRef, CreatedAt
            FROM ful.Shipments
            WHERE OrderId = @OrderId;";

        return await _connection.QuerySingleOrDefaultAsync<ShipmentSummaryResponse>(
            new CommandDefinition(sql, new { query.OrderId }, cancellationToken: ct));
    }
}
