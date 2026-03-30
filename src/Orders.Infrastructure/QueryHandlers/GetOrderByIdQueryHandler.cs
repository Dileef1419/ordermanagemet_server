using System.Data;
using Dapper;
using Orders.Application.DTOs;
using Orders.Application.Queries.GetOrderById;

namespace Orders.Infrastructure.QueryHandlers;

/// <summary>
/// Dapper-based read model handler — bypasses EF Core for performance.
/// </summary>
public class GetOrderByIdQueryHandler : IGetOrderByIdQueryHandler
{
    private readonly IDbConnection _connection;

    public GetOrderByIdQueryHandler(IDbConnection connection) => _connection = connection;

    public async Task<DetailedOrderResponse?> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                o.Id AS OrderId, o.CustomerName, o.CustomerId, o.Status, o.TotalAmount, o.Currency, o.PlacedAt,
                o.ShippingAddress_FullName AS FullName, 
                o.ShippingAddress_AddressLine1 AS AddressLine1, 
                o.ShippingAddress_City AS City, 
                o.ShippingAddress_PostalCode AS PostalCode, 
                o.ShippingAddress_Country AS Country,
                l.Sku, l.Quantity, l.UnitPrice
            FROM ord.Orders o
            LEFT JOIN ord.OrderLines l ON o.Id = l.OrderId
            WHERE o.Id = @OrderId;";

        var orderDict = new Dictionary<Guid, DetailedOrderResponse>();

        var result = await _connection.QueryAsync<dynamic, AddressDto, OrderDetailLineResponse, DetailedOrderResponse>(
            sql,
            (o, addr, line) =>
            {
                if (!orderDict.TryGetValue(o.OrderId, out DetailedOrderResponse? order))
                {
                    order = new DetailedOrderResponse(
                        o.OrderId, o.CustomerName, o.CustomerId, o.Status, o.TotalAmount, o.Currency, o.PlacedAt,
                        addr, new List<OrderDetailLineResponse>());
                    orderDict.Add(o.OrderId, order);
                }
                
                if (line != null && order != null)
                    order.Lines.Add(line);
                
                return order!;
            },
            new { query.OrderId },
            splitOn: "FullName,Sku");

        return orderDict.Values.FirstOrDefault();
    }
}
