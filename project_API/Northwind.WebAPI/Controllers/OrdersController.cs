using Microsoft.AspNetCore.Mvc;
using Northwind.Application.DTOs;
using Northwind.Application.Intefaces;
using project_API.Northwind.WebAPI.Helpers;

namespace project_API.Northwind.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get all orders, optionally filtered by year, month, week, and region.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int? week,
        [FromQuery] string? region)
    {
        var orders = await _orderService.GetAllOrdersAsync();
        var filtered = orders.AsEnumerable();

        if (year.HasValue)
            filtered = filtered.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == year.Value);
        if (month.HasValue)
            filtered = filtered.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Month == month.Value);
        if (week.HasValue)
        {
            filtered = filtered.Where(o =>
            {
                if (!o.OrderDate.HasValue) return false;
                var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
                var wk = cal.GetWeekOfYear(o.OrderDate.Value, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                return wk == week.Value;
            });
        }
        if (!string.IsNullOrWhiteSpace(region))
            filtered = filtered.Where(o => string.Equals(o.ShipRegion, region, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(o.ShipCountry, region, StringComparison.OrdinalIgnoreCase));

        return Ok(filtered.ToList());
    }

    /// <summary>
    /// Get a specific order by its ID, including line items.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    /// <summary>
    /// Create a new order with employee assignment, freight, and line items.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var orderId = await _orderService.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { OrderId = orderId });
    }

    /// <summary>
    /// Update an existing order (status, address, freight, etc).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] CreateOrderDto dto)
    {
        var success = await _orderService.UpdateOrderAsync(id, dto);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Delete an order by ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var success = await _orderService.DeleteOrderAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Export filtered orders to an Excel spreadsheet.
    /// </summary>
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string? region)
    {
        var orders = await _orderService.GetAllOrdersAsync();
        var filtered = orders.AsEnumerable();
        if (year.HasValue) filtered = filtered.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == year.Value);
        if (month.HasValue) filtered = filtered.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Month == month.Value);
        if (!string.IsNullOrWhiteSpace(region))
            filtered = filtered.Where(o => string.Equals(o.ShipRegion, region, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(o.ShipCountry, region, StringComparison.OrdinalIgnoreCase));

        var list = filtered.ToList();
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Orders");

        var headers = new[] { "Order ID", "Customer", "Date", "Status", "Region", "Freight", "Total" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#000000");
            worksheet.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        }

        int row = 2;
        foreach (var order in list)
        {
            worksheet.Cell(row, 1).Value = order.OrderId;
            worksheet.Cell(row, 2).Value = order.CustomerName;
            worksheet.Cell(row, 3).Value = order.OrderDate?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 4).Value = order.ShippedDate.HasValue ? "Delivered" : "In Transit";
            worksheet.Cell(row, 5).Value = order.ShipRegion ?? order.ShipCountry ?? "Unknown";
            worksheet.Cell(row, 6).Value = order.Freight ?? 0;
            worksheet.Cell(row, 7).Value = order.TotalAmount;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NorthwindOrders.xlsx");
    }

    /// <summary>
    /// Export an order details report to a branded PDF document.
    /// Pass orderId query param for single-order report, omit for summary.
    /// </summary>
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] int? orderId)
    {
        if (orderId.HasValue)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId.Value);
            if (order == null) return NotFound();

            byte[] pdfBytes = PdfGenerator.GenerateOrderReport(order);
            return File(pdfBytes, "application/pdf", $"Order_{order.OrderId}_Report.pdf");
        }
        else
        {
            var orders = await _orderService.GetAllOrdersAsync();
            byte[] pdfBytes = PdfGenerator.GenerateSummaryReport(orders.ToList());
            return File(pdfBytes, "application/pdf", "NorthwindOrdersReport.pdf");
        }
    }
}