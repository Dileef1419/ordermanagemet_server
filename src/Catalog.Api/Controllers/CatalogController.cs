using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

/// <summary>
/// Catalog Controller — pure read-only service (simulating Cosmos DB).
/// Non-relational data store pattern.
/// </summary>
[ApiController]
[Route("api/v1/catalog")]
[Produces("application/json")]
public class CatalogController : ControllerBase
{
    private readonly IProductStore _store;

    public CatalogController(IProductStore store) => _store = store;

    /// <summary>Get product by ID.</summary>
    [HttpGet("products/{productId}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(string productId)
    {
        var product = await _store.GetByIdAsync(productId);
        return product is not null ? Ok(product) : NotFound();
    }

    /// <summary>Search products by category and/or text.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? category,
        [FromQuery] string? search)
    {
        var products = await _store.SearchAsync(category, search);
        return Ok(products);
    }

    /// <summary>Get price list by category.</summary>
    [HttpGet("price-list/{categoryId}")]
    [ProducesResponseType(typeof(IReadOnlyList<PriceListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPriceList(string categoryId)
    {
        var prices = await _store.GetPriceListAsync(categoryId);
        return Ok(prices);
    }
}
