using Microsoft.AspNetCore.Mvc;
using Northwind.Application.Intefaces;

namespace project_API.Northwind.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all active (non-discontinued) products for autocomplete lookup.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] string? search)
    {
        var products = await _productService.GetAllProductsAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var query = search.ToLowerInvariant();
            products = products.Where(p =>
                p.ProductId.ToString().Contains(query) ||
                p.ProductName.ToLowerInvariant().Contains(query));
        }

        return Ok(products);
    }
}
