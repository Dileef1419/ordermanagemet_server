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
        Description = "Product catalog read service (Cosmos DB simulation)"
    });
});

builder.Services.AddHealthChecks();

// In production: Cosmos DB client registration
// builder.Services.AddSingleton(sp => new CosmosClient(connectionString));
// For local dev: in-memory product store
builder.Services.AddSingleton<IProductStore, InMemoryProductStore>();

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

// ===== Catalog Models & In-Memory Store (represents Cosmos DB read model) =====

public record ProductDto(
    string Id,
    string CategoryId,
    string Name,
    string Sku,
    decimal Price,
    string Currency,
    int Available);

public record PriceListItem(string Sku, string Name, decimal Price, string Currency);

public interface IProductStore
{
    Task<ProductDto?> GetByIdAsync(string productId);
    Task<IReadOnlyList<ProductDto>> SearchAsync(string? category, string? searchTerm);
    Task<IReadOnlyList<PriceListItem>> GetPriceListAsync(string categoryId);
}

/// <summary>
/// In-memory implementation simulating Cosmos DB document store.
/// In production, this would query Azure Cosmos DB NoSQL API.
/// </summary>
public class InMemoryProductStore : IProductStore
{
    private readonly List<ProductDto> _products = new()
    {
        new("prod-001", "electronics", "Wireless Mouse", "WM-2026-BLK", 49.99m, "AUD", 230),
        new("prod-002", "electronics", "Mechanical Keyboard", "MK-2026-RGB", 149.99m, "AUD", 85),
        new("prod-003", "electronics", "USB-C Hub", "UCH-2026", 79.99m, "AUD", 150),
        new("prod-004", "accessories", "Laptop Stand", "LS-ALU-SLV", 89.99m, "AUD", 60),
        new("prod-005", "accessories", "Webcam HD", "WC-1080P", 129.99m, "AUD", 40),
    };

    public Task<ProductDto?> GetByIdAsync(string productId)
        => Task.FromResult(_products.FirstOrDefault(p => p.Id == productId));

    public Task<IReadOnlyList<ProductDto>> SearchAsync(string? category, string? searchTerm)
    {
        var query = _products.AsEnumerable();
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.CategoryId.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<IReadOnlyList<ProductDto>>(query.ToList());
    }

    public Task<IReadOnlyList<PriceListItem>> GetPriceListAsync(string categoryId)
    {
        var prices = _products
            .Where(p => p.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase))
            .Select(p => new PriceListItem(p.Sku, p.Name, p.Price, p.Currency))
            .ToList();
        return Task.FromResult<IReadOnlyList<PriceListItem>>(prices);
    }
}
