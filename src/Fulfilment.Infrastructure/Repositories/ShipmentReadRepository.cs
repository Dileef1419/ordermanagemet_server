using System.Data;
using Dapper;
using Fulfilment.Domain.Repositories;

namespace Fulfilment.Infrastructure.Repositories;

public class ShipmentReadRepository : IShipmentReadRepository
{
    private readonly IDbConnection _connection;
    public ShipmentReadRepository(IDbConnection connection) => _connection = connection;

    public async Task<ShipmentSummaryDto?> GetByOrderIdAsync(Guid orderId, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id AS ShipmentId, OrderId, WarehouseId, Status,
                   TrackingNumber, CarrierRef, CreatedAt
            FROM ful.Shipments
            WHERE OrderId = @OrderId;";

        return await _connection.QuerySingleOrDefaultAsync<ShipmentSummaryDto>(
            new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<WarehouseQueueDto>> GetWarehouseQueueAsync(
        int warehouseId, string? status, int page, int pageSize, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id AS ShipmentId, OrderId, Status, CreatedAt
            FROM ful.Shipments
            WHERE WarehouseId = @WarehouseId
              AND (@Status IS NULL OR Status = @Status)
            ORDER BY CreatedAt ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var results = await _connection.QueryAsync<WarehouseQueueDto>(
            new CommandDefinition(sql, new
            {
                WarehouseId = warehouseId,
                Status = status,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            }, cancellationToken: ct));
        return results.ToList();
    }
}
