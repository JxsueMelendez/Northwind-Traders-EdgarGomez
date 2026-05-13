using Northwind.Domain.Entities;

namespace Northwind.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    private static string? TrimTo(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return value.Length > maxLength ? value.Substring(0, maxLength) : value;
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null) return null;

        return new OrderResponseDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.CompanyName,
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            TotalAmount = order.CalculateTotal(),
            ShipRegion = order.ShipRegion,
            ShipCity = order.ShipCity,
            ShipCountry = order.ShipCountry,
            ShipAddress = order.ShipAddress,
            ShippedDate = order.ShippedDate,
            EmployeeId = order.EmployeeId,
            Freight = order.Freight,
            LineItems = order.OrderDetails?.Select(d => new OrderDetailDto
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Discount = d.Discount
            }).ToList() ?? new List<OrderDetailDto>()
        };
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _repository.GetAllAsync();
        return orders.Select(o => new OrderResponseDto
        {
            OrderId = o.OrderId,
            CustomerId = o.CustomerId,
            OrderDate = o.OrderDate,
            RequiredDate = o.RequiredDate,
            CustomerName = o.Customer?.CompanyName,
            TotalAmount = o.CalculateTotal(),
            ShipRegion = o.ShipRegion,
            ShipCity = o.ShipCity,
            ShipCountry = o.ShipCountry,
            ShipAddress = o.ShipAddress,
            ShippedDate = o.ShippedDate,
            EmployeeId = o.EmployeeId,
            Freight = o.Freight
        });
    }

    public async Task<int> CreateOrderAsync(CreateOrderDto dto)
    {
        var details = dto.Details ?? new List<CreateOrderDetailDto>();
        var freight = dto.Freight ?? 0m;

        var newOrder = new Order
        {
            CustomerId = dto.CustomerId,
            EmployeeId = dto.EmployeeId,
            OrderDate = DateTime.UtcNow,
            ShipAddress = TrimTo(dto.AddressLine, 60),
            ShipCity = TrimTo(dto.City, 15),
            ShipRegion = TrimTo(dto.Region, 15),
            ShipCountry = TrimTo(dto.Country, 15),
            Freight = freight,
            OrderDetails = details.Select(d => new OrderDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Discount = 0
            }).ToList()
        };

        var createdOrder = await _repository.AddAsync(newOrder);
        return createdOrder.OrderId;
    }

    public async Task<bool> UpdateOrderAsync(int id, CreateOrderDto dto)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        var details = dto.Details ?? new List<CreateOrderDetailDto>();
        var freight = dto.Freight ?? 0m;

        order.CustomerId = dto.CustomerId;
        order.EmployeeId = dto.EmployeeId;
        order.ShipAddress = TrimTo(dto.AddressLine, 60);
        order.ShipCity = TrimTo(dto.City, 15);
        order.ShipRegion = TrimTo(dto.Region, 15);
        order.ShipCountry = TrimTo(dto.Country, 15);
        order.Freight = freight;
        order.ShippedDate = dto.ShippedDate;
        order.OrderDetails = details.Select(d => new OrderDetail
        {
            OrderId = id,
            ProductId = d.ProductId,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice
        }).ToList();

        await _repository.UpdateAsync(order);
        return true;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        await _repository.DeleteAsync(id);
        return true;
    }
}