using Microsoft.EntityFrameworkCore;
using Northwind.Domain.Entities;
using Northwind.Infrastructure.Persistence;
using Northwind.Infrastructure.Repositories;

namespace Northwind.Tests;

public class RepositoryTests
{
    private NorthwindDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<NorthwindDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new NorthwindDbContext(options);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    [Fact]
    public async Task OrderRepository_AddAsync_ShouldAddOrder()
    {
        // Arrange
        var context = GetDatabaseContext();
        var repository = new OrderRepository(context);
        var order = new Order { CustomerId = "ALFKI", OrderDate = DateTime.Now };

        // Act
        var result = await repository.AddAsync(order);

        // Assert
        Assert.NotEqual(0, result.OrderId);
        Assert.Equal(1, await context.Orders.CountAsync());
    }

    [Fact]
    public async Task OrderRepository_GetByIdAsync_ShouldReturnOrderWithDetails()
    {
        // Arrange
        var context = GetDatabaseContext();
        var repository = new OrderRepository(context);
        var order = new Order 
        { 
            CustomerId = "ALFKI", 
            OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = 1, Quantity = 1 } } 
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(order.OrderId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.OrderDetails);
    }

    [Fact]
    public async Task OrderRepository_UpdateAsync_ShouldReplaceDetails()
    {
        // Arrange
        var context = GetDatabaseContext();
        var repository = new OrderRepository(context);
        var order = new Order 
        { 
            CustomerId = "ALFKI", 
            OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = 1, Quantity = 1 } } 
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Update details
        order.OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = 2, Quantity = 5 } };

        // Act
        await repository.UpdateAsync(order);
        var result = await repository.GetByIdAsync(order.OrderId);

        // Assert
        Assert.Single(result.OrderDetails);
        Assert.Equal(2, result.OrderDetails.First().ProductId);
    }

    [Fact]
    public async Task OrderRepository_DeleteAsync_ShouldRemoveOrder()
    {
        // Arrange
        var context = GetDatabaseContext();
        var repository = new OrderRepository(context);
        var order = new Order { CustomerId = "ALFKI" };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(order.OrderId);

        // Assert
        Assert.Equal(0, await context.Orders.CountAsync());
    }

    [Fact]
    public async Task OrderRepository_GetAllAsync_ShouldIncludeOrders()
    {
        var context = GetDatabaseContext();
        context.Orders.AddRange(
            new Order { CustomerId = "A" },
            new Order { CustomerId = "B" });
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context);
        var result = (await repository.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CustomerRepository_GetAllAsync_ShouldReturnOrderedCustomers()
    {
        var context = GetDatabaseContext();
        context.Customers.AddRange(
            new Customer { CustomerId = "B", CompanyName = "Beta" },
            new Customer { CustomerId = "A", CompanyName = "Alpha" });
        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);
        var result = (await repository.GetAllAsync()).ToList();

        Assert.Equal("Alpha", result[0].CompanyName);
        Assert.Equal("Beta", result[1].CompanyName);
    }

    [Fact]
    public async Task EmployeeRepository_GetAllAsync_ShouldReturnOrderedEmployees()
    {
        var context = GetDatabaseContext();
        context.Employees.AddRange(
            new Employee { EmployeeId = 2, LastName = "Zulu" },
            new Employee { EmployeeId = 1, LastName = "Alpha" });
        await context.SaveChangesAsync();

        var repository = new EmployeeRepository(context);
        var result = (await repository.GetAllAsync()).ToList();

        Assert.Equal("Alpha", result[0].LastName);
        Assert.Equal("Zulu", result[1].LastName);
    }

    [Fact]
    public async Task ProductRepository_GetAllAsync_ShouldExcludeDiscontinued()
    {
        var context = GetDatabaseContext();
        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "Apple", Discontinued = false },
            new Product { ProductId = 2, ProductName = "Old", Discontinued = true });
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var result = (await repository.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].ProductId);
    }
}
