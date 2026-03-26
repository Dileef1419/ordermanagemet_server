using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("CatalogDb")));

        return services;
    }
}