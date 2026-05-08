using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using project_API.Northwind.WebAPI.Controllers;

namespace Northwind.Tests;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _controller = new OrdersController(_mockOrderService.Object);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnOkResult_WithListOfOrders()
    {
        var orders = new List<OrderResponseDto>
        {
            new OrderResponseDto { OrderId = 1, OrderDate = new DateTime(2026, 1, 15) },
            new OrderResponseDto { OrderId = 2, OrderDate = new DateTime(2026, 3, 10) }
        };
        _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

        var result = await _controller.GetAllOrders(null, null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnOrders = Assert.IsAssignableFrom<IEnumerable<OrderResponseDto>>(okResult.Value);
        Assert.Equal(2, returnOrders.Count());
    }

    [Fact]
    public async Task GetAllOrders_ShouldFilterByYear()
    {
        var orders = new List<OrderResponseDto>
        {
            new OrderResponseDto { OrderId = 1, OrderDate = new DateTime(2025, 5, 1) },
            new OrderResponseDto { OrderId = 2, OrderDate = new DateTime(2026, 1, 10) }
        };
        _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

        var result = await _controller.GetAllOrders(2026, null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnOrders = Assert.IsAssignableFrom<IEnumerable<OrderResponseDto>>(okResult.Value);
        Assert.Single(returnOrders);
        Assert.Equal(2, returnOrders.First().OrderId);
    }

    [Fact]
    public async Task GetAllOrders_ShouldFilterByRegion()
    {
        var orders = new List<OrderResponseDto>
        {
            new OrderResponseDto { OrderId = 1, ShipRegion = "Western Europe", ShipCountry = "Germany" },
            new OrderResponseDto { OrderId = 2, ShipRegion = "North America", ShipCountry = "USA" }
        };
        _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

        var result = await _controller.GetAllOrders(null, null, null, "Germany");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnOrders = Assert.IsAssignableFrom<IEnumerable<OrderResponseDto>>(okResult.Value);
        Assert.Single(returnOrders);
        Assert.Equal(1, returnOrders.First().OrderId);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnOkResult_WhenOrderExists()
    {
        var order = new OrderResponseDto { OrderId = 1, EmployeeId = 5, Freight = 32.38m };
        _mockOrderService.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync(order);

        var result = await _controller.GetOrderById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnOrder = Assert.IsType<OrderResponseDto>(okResult.Value);
        Assert.Equal(1, returnOrder.OrderId);
        Assert.Equal(5, returnOrder.EmployeeId);
        Assert.Equal(32.38m, returnOrder.Freight);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        _mockOrderService.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync((OrderResponseDto?)null);

        var result = await _controller.GetOrderById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnCreatedAtAction()
    {
        var dto = new CreateOrderDto { EmployeeId = 3, Freight = 15.00m };
        _mockOrderService.Setup(s => s.CreateOrderAsync(dto)).ReturnsAsync(1);

        var result = await _controller.CreateOrder(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetOrderById), createdResult.ActionName);
        Assert.Equal(1, createdResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnNoContent_WhenSuccessful()
    {
        var dto = new CreateOrderDto { EmployeeId = 2, Freight = 5.50m };
        _mockOrderService.Setup(s => s.UpdateOrderAsync(1, dto)).ReturnsAsync(true);

        var result = await _controller.UpdateOrder(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnNotFound_WhenUnsuccessful()
    {
        var dto = new CreateOrderDto();
        _mockOrderService.Setup(s => s.UpdateOrderAsync(1, dto)).ReturnsAsync(false);

        var result = await _controller.UpdateOrder(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturnNoContent_WhenSuccessful()
    {
        _mockOrderService.Setup(s => s.DeleteOrderAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteOrder(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturnNotFound_WhenUnsuccessful()
    {
        _mockOrderService.Setup(s => s.DeleteOrderAsync(1)).ReturnsAsync(false);

        var result = await _controller.DeleteOrder(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
