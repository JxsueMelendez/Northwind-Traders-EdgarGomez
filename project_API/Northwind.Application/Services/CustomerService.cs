using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using Northwind.Domain.Entities;

namespace Northwind.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _repository.GetAllAsync();
        return (customers ?? Enumerable.Empty<Customer>())
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId ?? string.Empty,
                CompanyName = c.CompanyName ?? "Unknown",
                ContactName = c.ContactName
            })
            .OrderBy(c => c.CompanyName);
    }
}
