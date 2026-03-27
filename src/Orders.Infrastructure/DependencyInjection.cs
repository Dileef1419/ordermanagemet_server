using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure
{
    public static class OrdersInfrastructureDependencyInjection
    {
        public static IServiceCollection AddOrdersInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            // 1. EF Core Write Model
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("OrdersDb")));

            // 2. Dapper Read Model (SqlConnection)
            services.AddScoped<IDbConnection>(sp => 
                new Microsoft.Data.SqlClient.SqlConnection(config.GetConnectionString("OrdersReadDb")));

            // 3. Command Handlers
            services.AddScoped<Orders.Application.Commands.PlaceOrder.IPlaceOrderCommandHandler, CommandHandlers.PlaceOrderHandler>();
            services.AddScoped<Orders.Application.Commands.CancelOrder.ICancelOrderCommandHandler, CommandHandlers.CancelOrderHandler>();
            services.AddScoped<Orders.Application.Commands.ConfirmOrder.IConfirmOrderCommandHandler, CommandHandlers.ConfirmOrderHandler>();
            services.AddScoped<Orders.Application.Commands.MarkOrderFailed.IMarkOrderFailedCommandHandler, CommandHandlers.MarkOrderFailedHandler>();

            // 4. Query Handlers
            services.AddScoped<Orders.Application.Queries.GetOrderById.IGetOrderByIdQueryHandler, QueryHandlers.GetOrderByIdQueryHandler>();
            services.AddScoped<Orders.Application.Queries.GetOrdersByCustomer.IGetOrdersByCustomerQueryHandler, QueryHandlers.GetOrdersByCustomerQueryHandler>();
            services.AddScoped<Orders.Application.Queries.GetDashboard.IGetDashboardQueryHandler, QueryHandlers.GetDashboardQueryHandler>();

            return services;
        }
    }
}