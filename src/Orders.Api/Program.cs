using SharedKernel.Filters;
using SharedKernel.Middleware;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure;
using Orders.Application;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── MVC Controllers + Filters ──
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// ── Swagger / OpenAPI ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Orders API",
        Version = "v1",
        Description = "Orders management service"
    });
});

// ── Application Layer (validators) ──
builder.Services.AddOrdersApplication();

// ── Infrastructure Layer ──
// Orders Infrastructure
builder.Services.AddOrdersInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Swagger (Development only) ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ── Middleware Pipeline ──
app.UseMiddleware<CorrelationIdMiddleware>();

// ── Endpoints ──
app.MapHealthChecks("/health");
// ── Database Startup ──
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Orders.Infrastructure.Persistence.OrdersDbContext>();

    // Manually ensure columns exist to avoid migration mismatch in development
    context.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND TABLE_SCHEMA = 'ord' AND COLUMN_NAME = 'ShippingAddress_FullName')
        BEGIN
            ALTER TABLE ord.Orders ADD ShippingAddress_FullName NVARCHAR(200) NULL;
            ALTER TABLE ord.Orders ADD ShippingAddress_AddressLine1 NVARCHAR(500) NULL;
            ALTER TABLE ord.Orders ADD ShippingAddress_City NVARCHAR(100) NULL;
            ALTER TABLE ord.Orders ADD ShippingAddress_PostalCode NVARCHAR(20) NULL;
            ALTER TABLE ord.Orders ADD ShippingAddress_Country NVARCHAR(100) NULL;
        END");

    if (!context.Orders.Any())
    {
        var dummyCustomerId = Guid.NewGuid();
        var order = Orders.Domain.Aggregates.Order.Place(
            dummyCustomerId,
            "Initial Test Customer",
            new Orders.Domain.Aggregates.Address("Test Customer", "123 Test St", "Sydney", "2000", "Australia"),
            new List<Orders.Domain.Aggregates.OrderLineInput>
        {
            new Orders.Domain.Aggregates.OrderLineInput("GMS-001", 1, 2500)
        });
        context.Orders.Add(order);
        context.SaveChanges();
    }
}

app.MapControllers();

app.Run();