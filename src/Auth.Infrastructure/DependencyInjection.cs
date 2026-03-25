using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auth.Application.Commands.Login;
using Auth.Application.Commands.Register;
using Auth.Application.Security;
using Auth.Domain.Repositories;
using Auth.Infrastructure.CommandHandlers;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Security;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core (Write/Read Model) ──
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("AuthDb"),
                sql => sql.EnableRetryOnFailure(3)));

        // ── Repositories ──
        services.AddScoped<IUserRepository, UserRepository>();

        // ── Command Handlers ──
        services.AddScoped<IRegisterUserCommandHandler, RegisterUserHandler>();
        services.AddScoped<ILoginUserCommandHandler, LoginUserHandler>();

        // ── JWT ──
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
