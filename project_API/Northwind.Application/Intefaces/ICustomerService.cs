using Northwind.Application.DTOs;

namespace Northwind.Application.Intefaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
}
