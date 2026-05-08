using Northwind.Application.Intefaces;
using Northwind.Application.DTOs;
using Northwind.Domain.Entities;

namespace Northwind.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
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
        var newOrder = new Order
        {
            CustomerId = dto.CustomerId,
            EmployeeId = dto.EmployeeId,
            OrderDate = DateTime.UtcNow,
            ShipAddress = dto.AddressLine,
            ShipCity = dto.City,
            ShipRegion = dto.Region,
            ShipCountry = dto.Country,
            Freight = dto.Freight,
            OrderDetails = dto.Details.Select(d => new OrderDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice
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

        order.CustomerId = dto.CustomerId;
        order.EmployeeId = dto.EmployeeId;
        order.ShipAddress = dto.AddressLine;
        order.ShipCity = dto.City;
        order.ShipRegion = dto.Region;
        order.ShipCountry = dto.Country;
        order.Freight = dto.Freight;
        order.ShippedDate = dto.ShippedDate;
        order.OrderDetails = dto.Details.Select(d => new OrderDetail
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