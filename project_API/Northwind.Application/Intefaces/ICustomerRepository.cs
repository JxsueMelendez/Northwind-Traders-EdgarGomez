using Northwind.Domain.Entities;

namespace Northwind.Application.Intefaces;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
}
