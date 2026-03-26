using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("PaymentsDb")));

        return services;
    }
}