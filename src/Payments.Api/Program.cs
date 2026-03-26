using Payments.Application;
using Payments.Infrastructure;
using SharedKernel.Filters;
using SharedKernel.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers + Global Filters ──
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
        Title = "Payments API",
        Version = "v1",
        Description = "Payment processing service — Authorise, Capture, Refund"
    });
});

// ── Application Layer ──
builder.Services.AddPaymentsApplication();

// ── Infrastructure Layer ──
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

// ── Health Checks ──
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Swagger UI (Development Only) ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ── Middleware Pipeline ──
app.UseMiddleware<CorrelationIdMiddleware>();

// ── Endpoints ──
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();