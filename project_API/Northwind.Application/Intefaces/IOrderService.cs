using Northwind.Application.DTOs;

namespace Northwind.Application.Intefaces;

public interface IOrderService
{
    Task<OrderResponseDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
    Task<int> CreateOrderAsync(CreateOrderDto dto);
    Task<bool> UpdateOrderAsync(int id, CreateOrderDto dto);
    Task<bool> DeleteOrderAsync(int id);
}