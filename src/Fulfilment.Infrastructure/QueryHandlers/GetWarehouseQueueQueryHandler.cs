using System.Data;
using Dapper;
using Fulfilment.Application.DTOs;
using Fulfilment.Application.Queries.GetWarehouseQueue;

namespace Fulfilment.Infrastructure.QueryHandlers;

public class GetWarehouseQueueQueryHandler : IGetWarehouseQueueQueryHandler
{
    private readonly IDbConnection _connection;
    public GetWarehouseQueueQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<IReadOnlyList<WarehouseQueueResponse>> Handle(
        GetWarehouseQueueQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id AS ShipmentId, OrderId, Status, CreatedAt
            FROM ful.Shipments
            WHERE WarehouseId = @WarehouseId
              AND (@Status IS NULL OR Status = @Status)
            ORDER BY CreatedAt ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var results = await _connection.QueryAsync<WarehouseQueueResponse>(
            new CommandDefinition(sql, new
            {
                query.WarehouseId,
                query.Status,
                Offset = (query.Page - 1) * query.PageSize,
                query.PageSize
            }, cancellationToken: ct));
        return results.ToList();
    }
}
