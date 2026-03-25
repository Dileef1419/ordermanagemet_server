using Microsoft.AspNetCore.RateLimiting;
using Orders.Application;
using Orders.Infrastructure;
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
        Title = "Orders API",
        Version = "v1",
        Description = "Order management service — Commands (EF Core) & Queries (Dapper)"
    });
});

// ── Application Layer (validators) ──
builder.Services.AddOrdersApplication();

// ── Infrastructure Layer (EF Core, Dapper, repos, handlers, background services) ──
builder.Services.AddOrdersInfrastructure(builder.Configuration);

// ── Rate Limiting ──
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("default", config =>
    {
        config.PermitLimit = 300;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 10;
    });
});

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
app.UseRateLimiter();

// ── Endpoints ──
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
public record CancelRequest(string Reason);
