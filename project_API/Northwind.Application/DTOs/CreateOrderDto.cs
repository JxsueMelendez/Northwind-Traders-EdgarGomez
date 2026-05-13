
using System.ComponentModel.DataAnnotations;

namespace Northwind.Application.DTOs;

public class CreateOrderDto
{
    [Required]
    [StringLength(5, MinimumLength = 1)]
    public string CustomerId { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }

    [Required]
    [StringLength(60, MinimumLength = 1)]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(15, MinimumLength = 1)]
    public string City { get; set; } = string.Empty;

    [StringLength(15)]
    public string? Region { get; set; }

    [Required]
    [StringLength(15, MinimumLength = 1)]
    public string Country { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? Freight { get; set; }

    public DateTime? ShippedDate { get; set; }

    [MinLength(1)]
    public List<CreateOrderDetailDto> Details { get; set; } = new();
}

public class CreateOrderDetailDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, short.MaxValue)]
    public short Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal UnitPrice { get; set; }
}