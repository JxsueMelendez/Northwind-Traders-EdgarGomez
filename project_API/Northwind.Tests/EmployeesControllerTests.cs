using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using project_API.Northwind.WebAPI.Controllers;

namespace Northwind.Tests;

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _mockEmployeeService = new Mock<IEmployeeService>();
        _controller = new EmployeesController(_mockEmployeeService.Object);
    }

    [Fact]
    public async Task GetAllEmployees_ShouldReturnAll_WhenSearchIsEmpty()
    {
        var employees = new List<EmployeeDto>
        {
            new EmployeeDto { EmployeeId = 1, FirstName = "John", LastName = "Smith" },
            new EmployeeDto { EmployeeId = 2, FirstName = "Jane", LastName = "Doe" }
        };
        _mockEmployeeService.Setup(s => s.GetAllEmployeesAsync()).ReturnsAsync(employees);

        var result = await _controller.GetAllEmployees(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnEmployees = Assert.IsAssignableFrom<IEnumerable<EmployeeDto>>(okResult.Value);
        Assert.Equal(2, returnEmployees.Count());
    }

    [Fact]
    public async Task GetAllEmployees_ShouldFilterBySearch()
    {
        var employees = new List<EmployeeDto>
        {
            new EmployeeDto { EmployeeId = 1, FirstName = "John", LastName = "Smith" },
            new EmployeeDto { EmployeeId = 2, FirstName = "Jane", LastName = "Doe" }
        };
        _mockEmployeeService.Setup(s => s.GetAllEmployeesAsync()).ReturnsAsync(employees);

        var result = await _controller.GetAllEmployees("doe");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnEmployees = Assert.IsAssignableFrom<IEnumerable<EmployeeDto>>(okResult.Value);
        Assert.Single(returnEmployees);
        Assert.Equal(2, returnEmployees.First().EmployeeId);
    }
}
