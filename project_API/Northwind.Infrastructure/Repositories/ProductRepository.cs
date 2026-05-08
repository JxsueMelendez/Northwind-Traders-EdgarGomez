using Microsoft.EntityFrameworkCore;
using Northwind.Application.Intefaces;
using Northwind.Domain.Entities;
using Northwind.Infrastructure.Persistence;

namespace Northwind.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly NorthwindDbContext _context;

    public ProductRepository(NorthwindDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Where(p => !p.Discontinued)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }
}
