using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using project_API.Northwind.WebAPI.Controllers;

namespace Northwind.Tests;

public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _controller = new CustomersController(_mockCustomerService.Object);
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnAll_WhenSearchIsEmpty()
    {
        var customers = new List<CustomerDto>
        {
            new CustomerDto { CustomerId = "ALFKI", CompanyName = "Alfreds" },
            new CustomerDto { CustomerId = "ANATR", CompanyName = "Ana Trujillo" }
        };
        _mockCustomerService.Setup(s => s.GetAllCustomersAsync()).ReturnsAsync(customers);

        var result = await _controller.GetAllCustomers(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnCustomers = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
        Assert.Equal(2, returnCustomers.Count());
    }

    [Fact]
    public async Task GetAllCustomers_ShouldFilterBySearch()
    {
        var customers = new List<CustomerDto>
        {
            new CustomerDto { CustomerId = "ALFKI", CompanyName = "Alfreds" },
            new CustomerDto { CustomerId = "ANATR", CompanyName = "Ana Trujillo" }
        };
        _mockCustomerService.Setup(s => s.GetAllCustomersAsync()).ReturnsAsync(customers);

        var result = await _controller.GetAllCustomers("alf");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnCustomers = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
        Assert.Single(returnCustomers);
        Assert.Equal("ALFKI", returnCustomers.First().CustomerId);
    }
}
