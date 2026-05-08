using Microsoft.AspNetCore.Mvc;
using Northwind.Application.Intefaces;

namespace project_API.Northwind.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Get all customers for autocomplete lookup.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCustomers([FromQuery] string? search)
    {
        var customers = await _customerService.GetAllCustomersAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var query = search.ToLowerInvariant();
            customers = customers.Where(c =>
                c.CustomerId.ToLowerInvariant().Contains(query) ||
                c.CompanyName.ToLowerInvariant().Contains(query));
        }

        return Ok(customers);
    }
}
