using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/v1/catalog")]
public class CatalogController : ControllerBase
{
    private readonly Catalog.Infrastructure.Persistence.CatalogDbContext _db;

    public CatalogController(Catalog.Infrastructure.Persistence.CatalogDbContext db)
    {
        _db = db;
    }

    public class ProductDto
    {
        public string? id { get; set; }
        public string name { get; set; } = default!;
        public string description { get; set; } = default!;
        public decimal price { get; set; }
        public string category { get; set; } = default!;
        public bool availability { get; set; }
        public string imageUrl { get; set; } = default!;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? term = null, [FromQuery] string? category = null, [FromQuery] bool? inStockOnly = null)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(p => p.Name.Contains(term));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.CategoryId == category);

        if (inStockOnly == true)
            query = query.Where(p => p.Available > 0);

        var list = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query);
        
        var items = list.Select(p => new ProductDto
        {
            id = p.Id, 
            name = p.Name, 
            description = "", 
            price = p.Price, 
            category = p.CategoryId, 
            availability = p.Available > 0, 
            imageUrl = "" 
        }).ToList();

        return Ok(new { items, total = items.Count });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProductDto { id = p.Id, name = p.Name, description = "", price = p.Price, category = p.CategoryId, availability = p.Available > 0, imageUrl = "" });
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        var p = new Catalog.Infrastructure.Persistence.Product
        {
            Name = dto.name,
            Price = dto.price,
            CategoryId = dto.category,
            Sku = Guid.NewGuid().ToString("N").Substring(0, 8),
            Available = dto.availability ? 10 : 0
        };
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> ToggleStock(string id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        p.Available = p.Available > 0 ? 0 : 10;
        await _db.SaveChangesAsync();
        return Ok(new ProductDto { id = p.Id, name = p.Name, description = "", price = p.Price, category = p.CategoryId, availability = p.Available > 0, imageUrl = "" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}