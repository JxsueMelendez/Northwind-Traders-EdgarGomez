using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using project_API.Northwind.WebAPI.Controllers;

namespace Northwind.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _controller = new ProductsController(_mockProductService.Object);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAll_WhenSearchIsEmpty()
    {
        var products = new List<ProductDto>
        {
            new ProductDto { ProductId = 1, ProductName = "Apple" },
            new ProductDto { ProductId = 2, ProductName = "Orange" }
        };
        _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(products);

        var result = await _controller.GetAllProducts(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Equal(2, returnProducts.Count());
    }

    [Fact]
    public async Task GetAllProducts_ShouldFilterBySearch()
    {
        var products = new List<ProductDto>
        {
            new ProductDto { ProductId = 1, ProductName = "Apple" },
            new ProductDto { ProductId = 2, ProductName = "Orange" }
        };
        _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(products);

        var result = await _controller.GetAllProducts("app");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Single(returnProducts);
        Assert.Equal(1, returnProducts.First().ProductId);
    }
}
