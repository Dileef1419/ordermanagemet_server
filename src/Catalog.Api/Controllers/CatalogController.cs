using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Catalog API working");
    }
}