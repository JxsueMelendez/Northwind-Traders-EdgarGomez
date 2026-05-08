using Microsoft.AspNetCore.Mvc;
using Northwind.Application.Intefaces;

namespace project_API.Northwind.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Get all employees for autocomplete lookup.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees([FromQuery] string? search)
    {
        var employees = await _employeeService.GetAllEmployeesAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var query = search.ToLowerInvariant();
            employees = employees.Where(e =>
                e.EmployeeId.ToString().Contains(query) ||
                e.FirstName.ToLowerInvariant().Contains(query) ||
                e.LastName.ToLowerInvariant().Contains(query));
        }

        return Ok(employees);
    }
}
