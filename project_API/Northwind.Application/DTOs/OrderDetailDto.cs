namespace Northwind.Application.DTOs;

public class OrderDetailDto
{
    public int ProductId { get; set; }
    public short Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public float Discount { get; set; }
    public decimal ExtendedPrice => (decimal)((float)UnitPrice * Quantity * (1 - Discount));
}
