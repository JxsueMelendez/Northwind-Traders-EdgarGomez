
namespace Northwind.Tests;

using Northwind.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void CalculateTotal_ShouldReturnCorrectSum_WhenOrderHasDetails()
    {
        var order = new Order
        {
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { UnitPrice = 10m, Quantity = 2, Discount = 0 },
                new OrderDetail { UnitPrice = 20m, Quantity = 1, Discount = 0.1f }
            }
        };

        var total = order.CalculateTotal();

        Assert.Equal(38m, total);
    }

    [Fact]
    public void CalculateTotal_ShouldReturnZero_WhenOrderHasNoDetails()
    {
        var order = new Order();

        var total = order.CalculateTotal();

        Assert.Equal(0m, total);
    }
}
