using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Orders.Application;

/// <summary>
/// Registers all Application-layer services (validators, etc.).
/// Command/Query handler implementations are registered in Infrastructure DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddOrdersApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
