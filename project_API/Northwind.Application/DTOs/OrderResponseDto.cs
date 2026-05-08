namespace Northwind.Application.DTOs;

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? ShipRegion { get; set; }
    public string? ShipCity { get; set; }
    public string? ShipCountry { get; set; }
    public string? ShipAddress { get; set; }
    public DateTime? ShippedDate { get; set; }
    public int? EmployeeId { get; set; }
    public decimal? Freight { get; set; }
    public List<OrderDetailDto> LineItems { get; set; } = new();
}