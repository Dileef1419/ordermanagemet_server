using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fulfilment.Application.Commands.CreateShipment;
using Fulfilment.Application.Commands.DispatchShipment;
using Fulfilment.Application.Queries.GetShipmentByOrder;
using Fulfilment.Application.Queries.GetWarehouseQueue;
using Fulfilment.Domain.Repositories;
using Fulfilment.Infrastructure.CommandHandlers;
using Fulfilment.Infrastructure.QueryHandlers;
using Fulfilment.Infrastructure.Repositories;

namespace Fulfilment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFulfilmentInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core (Write Model) ──
        services.AddDbContext<Persistence.FulfilmentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("FulfilmentDb"),
                sql => sql.EnableRetryOnFailure(3)));

        // ── Dapper (Read Model) ──
        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("FulfilmentReadDb")));

        // ── Repositories ──
        services.AddScoped<IShipmentReadRepository, ShipmentReadRepository>();

        // ── Command Handlers ──
        services.AddScoped<ICreateShipmentCommandHandler, CreateShipmentHandler>();
        services.AddScoped<IDispatchShipmentCommandHandler, DispatchShipmentHandler>();

        // ── Query Handlers ──
        services.AddScoped<IGetShipmentByOrderQueryHandler, GetShipmentByOrderQueryHandler>();
        services.AddScoped<IGetWarehouseQueueQueryHandler, GetWarehouseQueueQueryHandler>();

        return services;
    }
}
