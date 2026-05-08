using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;

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
        return customers
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CompanyName = c.CompanyName,
                ContactName = c.ContactName
            })
            .OrderBy(c => c.CompanyName);
    }
}
