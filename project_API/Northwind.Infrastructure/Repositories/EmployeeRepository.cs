using Microsoft.EntityFrameworkCore;
using Northwind.Application.Intefaces;
using Northwind.Domain.Entities;
using Northwind.Infrastructure.Persistence;

namespace Northwind.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly NorthwindDbContext _context;

    public EmployeeRepository(NorthwindDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }
}
