using Northwind.Application.DTOs;

namespace Northwind.Application.Intefaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
}
