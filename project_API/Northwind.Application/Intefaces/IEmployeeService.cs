using Northwind.Application.DTOs;

namespace Northwind.Application.Intefaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
}
