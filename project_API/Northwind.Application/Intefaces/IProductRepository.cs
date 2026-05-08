using Northwind.Domain.Entities;

namespace Northwind.Application.Intefaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
}
