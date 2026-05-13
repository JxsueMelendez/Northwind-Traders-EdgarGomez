using Moq;
using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using Northwind.Application.Services;
using Northwind.Domain.Entities;

namespace Northwind.Tests;

public class ApplicationServiceTests
{
    [Fact]
    public async Task CustomerService_GetAllCustomersAsync_ShouldReturnOrderedList()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer { CustomerId = "B", CompanyName = "B Company" },
            new Customer { CustomerId = "A", CompanyName = "A Company" }
        };
        var mockRepo = new Mock<ICustomerRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);
        var service = new CustomerService(mockRepo.Object);

        // Act
        var result = await service.GetAllCustomersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("A Company", result.First().CompanyName);
        Assert.Equal("B Company", result.Last().CompanyName);
    }

    [Fact]
    public async Task CustomerService_GetAllCustomersAsync_ShouldHandleNulls()
    {
        var customers = new List<Customer>
        {
            new Customer { CustomerId = null!, CompanyName = null!, ContactName = "Jane" }
        };
        var mockRepo = new Mock<ICustomerRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);
        var service = new CustomerService(mockRepo.Object);

        var result = await service.GetAllCustomersAsync();

        var customer = Assert.Single(result);
        Assert.Equal(string.Empty, customer.CustomerId);
        Assert.Equal("Unknown", customer.CompanyName);
        Assert.Equal("Jane", customer.ContactName);
    }

    [Fact]
    public async Task CustomerService_GetAllCustomersAsync_ShouldHandleNullCollection()
    {
        var mockRepo = new Mock<ICustomerRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync((IEnumerable<Customer>)null!);
        var service = new CustomerService(mockRepo.Object);

        var result = await service.GetAllCustomersAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task EmployeeService_GetAllEmployeesAsync_ShouldReturnOrderedList()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new Employee { EmployeeId = 2, FirstName = "Jane", LastName = "Doe" },
            new Employee { EmployeeId = 1, FirstName = "John", LastName = "Smith" }
        };
        var mockRepo = new Mock<IEmployeeRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);
        var service = new EmployeeService(mockRepo.Object);

        // Act
        var result = await service.GetAllEmployeesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Doe", result.First().LastName);
        Assert.Equal("Smith", result.Last().LastName);
    }

    [Fact]
    public async Task ProductService_GetAllProductsAsync_ShouldReturnOrderedList()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 2, ProductName = "Orange" },
            new Product { ProductId = 1, ProductName = "Apple" }
        };
        var mockRepo = new Mock<IProductRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        var service = new ProductService(mockRepo.Object);

        // Act
        var result = await service.GetAllProductsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Apple", result.First().ProductName);
        Assert.Equal("Orange", result.Last().ProductName);
    }

    [Fact]
    public async Task ProductService_GetAllProductsAsync_ShouldDefaultUnitPrice()
    {
        var products = new List<Product>
        {
            new Product { ProductId = 1, ProductName = "Apple", UnitPrice = null }
        };
        var mockRepo = new Mock<IProductRepository>();
        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        var service = new ProductService(mockRepo.Object);

        var result = await service.GetAllProductsAsync();

        var product = Assert.Single(result);
        Assert.Equal(0m, product.UnitPrice);
    }
}
