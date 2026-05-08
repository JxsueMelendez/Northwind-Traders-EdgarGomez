using Moq;
using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using Northwind.Application.Services;
using Northwind.Domain.Entities;

namespace Northwind.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _service = new OrderService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnOrderDto_WhenOrderExists()
    {
        var order = new Order
        {
            OrderId = 1,
            Customer = new Customer { CompanyName = "Test Co" },
            OrderDate = DateTime.UtcNow,
            RequiredDate = DateTime.UtcNow.AddDays(7),
            EmployeeId = 3,
            Freight = 25.50m,
            ShipRegion = "Western Europe",
            ShipCity = "Berlin",
            ShipCountry = "Germany",
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { UnitPrice = 10, Quantity = 2, Discount = 0 }
            }
        };

        _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _service.GetOrderByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.OrderId);
        Assert.Equal("Test Co", result.CustomerName);
        Assert.Equal(20, result.TotalAmount);
        Assert.Equal(3, result.EmployeeId);
        Assert.Equal(25.50m, result.Freight);
        Assert.Equal("Berlin", result.ShipCity);
        Assert.Single(result.LineItems);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Order?)null);

        var result = await _service.GetOrderByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnListOfOrderDtos()
    {
        var orders = new List<Order>
        {
            new Order { OrderId = 1, OrderDate = DateTime.UtcNow, EmployeeId = 1, Freight = 5m },
            new Order { OrderId = 2, OrderDate = DateTime.UtcNow, EmployeeId = 2, Freight = 10m }
        };

        _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(orders);

        var result = await _service.GetAllOrdersAsync();

        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0].EmployeeId);
        Assert.Equal(10m, list[1].Freight);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldMapAllFields_AndReturnOrderId()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = "TEST",
            EmployeeId = 5,
            AddressLine = "123 Test St",
            City = "Test City",
            Region = "Test Region",
            Country = "Testland",
            Freight = 33.50m,
            Details = new List<CreateOrderDetailDto>
            {
                new CreateOrderDetailDto { ProductId = 1, Quantity = 1, UnitPrice = 10 }
            }
        };

        var createdOrder = new Order { OrderId = 100 };
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Order>())).ReturnsAsync(createdOrder);

        var result = await _service.CreateOrderAsync(dto);

        Assert.Equal(100, result);
        _mockRepository.Verify(repo => repo.AddAsync(It.Is<Order>(o =>
            o.CustomerId == "TEST" &&
            o.EmployeeId == 5 &&
            o.ShipRegion == "Test Region" &&
            o.Freight == 33.50m
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldMapAllFields_WhenOrderExists()
    {
        var order = new Order { OrderId = 1 };
        var dto = new CreateOrderDto
        {
            CustomerId = "NEW",
            EmployeeId = 7,
            Region = "South America",
            Freight = 12.00m,
            ShippedDate = new DateTime(2026, 5, 1),
            Details = new List<CreateOrderDetailDto>()
        };

        _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _service.UpdateOrderAsync(1, dto);

        Assert.True(result);
        _mockRepository.Verify(repo => repo.UpdateAsync(order), Times.Once);
        Assert.Equal("NEW", order.CustomerId);
        Assert.Equal(7, order.EmployeeId);
        Assert.Equal("South America", order.ShipRegion);
        Assert.Equal(12.00m, order.Freight);
        Assert.Equal(new DateTime(2026, 5, 1), order.ShippedDate);
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var dto = new CreateOrderDto { Details = new List<CreateOrderDetailDto>() };
        _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Order?)null);

        var result = await _service.UpdateOrderAsync(1, dto);

        Assert.False(result);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task DeleteOrderAsync_ShouldReturnTrue_WhenOrderExists()
    {
        var order = new Order { OrderId = 1 };
        _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _service.DeleteOrderAsync(1);

        Assert.True(result);
        _mockRepository.Verify(repo => repo.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Order?)null);

        var result = await _service.DeleteOrderAsync(1);

        Assert.False(result);
        _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
