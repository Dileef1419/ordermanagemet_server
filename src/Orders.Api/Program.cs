using Payments.Application;
using Payments.Infrastructure;
using SharedKernel.Filters;
using SharedKernel.Middleware;
using Orders.Infrastructure; // <-- new Orders.Infrastructure namespace

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
builder.Services.AddPaymentsApplication(); // keep Payments app layer if needed

// ── Infrastructure Layer ──
// Payments Infrastructure
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

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
app.MapControllers();

app.Run();