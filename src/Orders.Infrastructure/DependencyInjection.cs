using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Commands.CancelOrder;
using Orders.Application.Commands.PlaceOrder;
using Orders.Application.Queries.GetDashboard;
using Orders.Application.Queries.GetOrderById;
using Orders.Application.Queries.GetOrdersByCustomer;
using Orders.Domain.Repositories;
using Orders.Infrastructure.BackgroundServices;
using Orders.Infrastructure.CommandHandlers;
using Orders.Infrastructure.QueryHandlers;
using Orders.Infrastructure.Repositories;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core (Write Model) ──
        services.AddDbContext<Persistence.OrdersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("OrdersDb"),
                sql => sql.EnableRetryOnFailure(3)));

        // ── Dapper (Read Model) ──
        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("OrdersReadDb")));

        // ── Repositories ──
        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();

        // ── Command Handlers ──
        services.AddScoped<IPlaceOrderCommandHandler, PlaceOrderHandler>();
        services.AddScoped<ICancelOrderCommandHandler, CancelOrderHandler>();

        // ── Query Handlers ──
        services.AddScoped<IGetOrderByIdQueryHandler, GetOrderByIdQueryHandler>();
        services.AddScoped<IGetOrdersByCustomerQueryHandler, GetOrdersByCustomerQueryHandler>();
        services.AddScoped<IGetDashboardQueryHandler, GetDashboardQueryHandler>();

        // ── Background Services ──
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }
}
