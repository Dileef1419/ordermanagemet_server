using Fulfilment.Application;
using Fulfilment.Infrastructure;
using SharedKernel.Filters;
using SharedKernel.Middleware;

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
        Title = "Fulfilment API",
        Version = "v1",
        Description = "Shipment & warehouse fulfilment service"
    });
});

// ── Application Layer (validators) ──
builder.Services.AddFulfilmentApplication();

// ── Infrastructure Layer (EF Core, Dapper, repos, handlers) ──
builder.Services.AddFulfilmentInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Swagger (Development only) ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fulfilment API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ── Middleware Pipeline ──
app.UseMiddleware<CorrelationIdMiddleware>();

// ── Endpoints ──
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
