using SharedKernel.Filters;
using SharedKernel.Middleware;
using Orders.Infrastructure;
using Orders.Application;

var builder = WebApplication.CreateBuilder(args);

// ── MVC Controllers + Filters ──
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
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
    context.Database.EnsureCreated();
    if (!context.Orders.Any())
    {
        var dummyCustomerId = Guid.NewGuid();
        var order = Orders.Domain.Aggregates.Order.Place(dummyCustomerId, "Initial Test Customer", new List<Orders.Domain.Aggregates.OrderLineInput>
        {
            new Orders.Domain.Aggregates.OrderLineInput("GMS-001", 1, 2500)
        });
        context.Orders.Add(order);
        context.SaveChanges();
    }
}

app.MapControllers();

app.Run();