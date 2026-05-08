using Microsoft.EntityFrameworkCore;
using Northwind.Application.Intefaces;
using Northwind.Domain.Entities;
using Northwind.Infrastructure.Persistence;

namespace Northwind.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly NorthwindDbContext _context;

    public CustomerRepository(NorthwindDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }
}
