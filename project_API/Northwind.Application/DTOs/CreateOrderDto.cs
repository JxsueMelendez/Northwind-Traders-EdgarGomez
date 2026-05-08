
namespace Northwind.Application.DTOs;

public class CreateOrderDto
{
    public string CustomerId { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string Country { get; set; } = string.Empty;
    public decimal? Freight { get; set; }
    public DateTime? ShippedDate { get; set; }
    public List<CreateOrderDetailDto> Details { get; set; } = new();
}

public class CreateOrderDetailDto
{
    public int ProductId { get; set; }
    public short Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}