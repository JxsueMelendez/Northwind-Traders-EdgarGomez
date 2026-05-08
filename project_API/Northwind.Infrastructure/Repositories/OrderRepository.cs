using Microsoft.EntityFrameworkCore;
using Northwind.Domain.Entities;
using Northwind.Application.Intefaces;
using Northwind.Infrastructure.Persistence;

namespace Northwind.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly NorthwindDbContext _context;

    public OrderRepository(NorthwindDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Shipper)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
            .ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        // Delete existing OrderDetails to avoid PK conflicts (OrderId+ProductId)
        var existingDetails = await _context.OrderDetails
            .Where(od => od.OrderId == order.OrderId)
            .ToListAsync();
        _context.OrderDetails.RemoveRange(existingDetails);

        // Update scalar fields on the order entity
        _context.Entry(order).State = EntityState.Modified;

        // Add new OrderDetails
        _context.OrderDetails.AddRange(order.OrderDetails);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}