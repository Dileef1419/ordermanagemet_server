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
            // Add your Orders DB context here
            // Example: services.AddDbContext<OrdersDbContext>(...)
            // Replace with actual OrdersDbContext connection if needed
            return services;
        }
    }
}