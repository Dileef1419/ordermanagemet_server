using Catalog.Infrastructure;
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
        Title = "Catalog API",
        Version = "v1",
        Description = "Product catalog service (SQL Server DB)"
    });
});

builder.Services.AddCatalogInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Swagger (Development only) ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ── Middleware Pipeline ──
app.UseMiddleware<CorrelationIdMiddleware>();

// ── Endpoints ──
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();