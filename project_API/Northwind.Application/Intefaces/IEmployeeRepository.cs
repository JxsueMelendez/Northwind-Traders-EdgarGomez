using Northwind.Domain.Entities;

namespace Northwind.Application.Intefaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
}
