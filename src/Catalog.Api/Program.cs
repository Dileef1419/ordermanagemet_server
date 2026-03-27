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
// ── Database Startup ──
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Catalog.Infrastructure.Persistence.CatalogDbContext>();
    context.Database.EnsureCreated();
    if (!context.Products.Any())
    {
        context.Products.AddRange(new List<Catalog.Infrastructure.Persistence.Product>
        {
            new Catalog.Infrastructure.Persistence.Product { Name = "Gaming Mouse", Sku = "GMS-001", CategoryId = "Electronics", Price = 2500, Available = 10 },
            new Catalog.Infrastructure.Persistence.Product { Name = "Mechanical Keyboard", Sku = "KBD-002", CategoryId = "Electronics", Price = 4500, Available = 5 },
            new Catalog.Infrastructure.Persistence.Product { Name = "Ergonomic Chair", Sku = "CHR-003", CategoryId = "Furniture", Price = 12000, Available = 0 }
        });
        context.SaveChanges();
    }
}
app.MapControllers();

app.Run();