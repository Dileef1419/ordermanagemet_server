using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Fulfilment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFulfilmentApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
