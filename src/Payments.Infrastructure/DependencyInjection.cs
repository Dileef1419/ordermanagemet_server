using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Commands.AuthorisePayment;
using Payments.Application.Commands.CapturePayment;
using Payments.Application.Commands.RefundPayment;
using Payments.Application.Queries.GetPaymentByOrder;
using Payments.Application.Queries.GetPaymentsByCustomer;
using Payments.Application.Queries.GetRevenue;
using Payments.Domain.Repositories;
using Payments.Infrastructure.CommandHandlers;
using Payments.Infrastructure.QueryHandlers;
using Payments.Infrastructure.Repositories;
using SharedKernel;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── Dapper DateOnly support ──
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        // ── EF Core (Write Model) ──
        services.AddDbContext<Persistence.PaymentsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("PaymentsDb"),
                sql => sql.EnableRetryOnFailure(3)));

        // ── Dapper (Read Model) ──
        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("PaymentsReadDb")));

        // ── Repositories ──
        services.AddScoped<IPaymentReadRepository, PaymentReadRepository>();

        // ── Command Handlers ──
        services.AddScoped<IAuthorisePaymentCommandHandler, AuthorisePaymentHandler>();
        services.AddScoped<ICapturePaymentCommandHandler, CapturePaymentHandler>();
        services.AddScoped<IRefundPaymentCommandHandler, RefundPaymentHandler>();

        // ── Query Handlers ──
        services.AddScoped<IGetPaymentByOrderQueryHandler, GetPaymentByOrderQueryHandler>();
        services.AddScoped<IGetPaymentsByCustomerQueryHandler, GetPaymentsByCustomerQueryHandler>();
        services.AddScoped<IGetRevenueQueryHandler, GetRevenueQueryHandler>();

        return services;
    }
}
