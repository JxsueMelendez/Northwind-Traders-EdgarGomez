using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Northwind.Application.DTOs;

namespace project_API.Northwind.WebAPI.Helpers;

public static class PdfGenerator
{
    public static byte[] GenerateOrderReport(OrderResponseDto order)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(col =>
                {
                    col.Item().Text("NORTHWIND TRADERS").Bold().FontSize(22);
                    col.Item().Text("Order Details Report").FontSize(14).FontColor("#666666");
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor("#eaeaea");
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Order #{order.OrderId}").Bold().FontSize(16);
                            c.Item().Text($"Customer: {order.CustomerName}");
                            c.Item().Text($"Date: {order.OrderDate?.ToString("yyyy-MM-dd")}");
                            c.Item().Text($"Status: {(order.ShippedDate.HasValue ? "Delivered" : "In Transit")}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Ship To: {order.ShipAddress}");
                            c.Item().Text($"City: {order.ShipCity}, {order.ShipCountry}");
                            c.Item().Text($"Region: {order.ShipRegion ?? "N/A"}");
                            c.Item().Text($"Freight: ${order.Freight:F2}");
                        });
                    });

                    col.Item().PaddingTop(15).Text("Line Items").Bold().FontSize(14);

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Product ID").Bold();
                            header.Cell().Text("Quantity").Bold();
                            header.Cell().Text("Unit Price").Bold();
                            header.Cell().Text("Discount").Bold();
                            header.Cell().Text("Subtotal").Bold();
                        });

                        foreach (var item in order.LineItems)
                        {
                            var subtotal = item.UnitPrice * item.Quantity * (1 - (decimal)item.Discount);
                            table.Cell().Text(item.ProductId.ToString());
                            table.Cell().Text(item.Quantity.ToString());
                            table.Cell().Text($"${item.UnitPrice:F2}");
                            table.Cell().Text($"{(item.Discount * 100):F0}%");
                            table.Cell().Text($"${subtotal:F2}");
                        }
                    });

                    col.Item().PaddingTop(10).AlignRight().Text($"Total: ${order.TotalAmount:F2}").Bold().FontSize(14);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Northwind Traders © 2026 — Generated on ");
                    t.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        });

        return document.GeneratePdf();
    }

    public static byte[] GenerateSummaryReport(List<OrderResponseDto> orders)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(col =>
                {
                    col.Item().Text("NORTHWIND TRADERS").Bold().FontSize(22);
                    col.Item().Text("Orders Summary Report").FontSize(14).FontColor("#666666");
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor("#eaeaea");
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Order ID").Bold();
                        header.Cell().Text("Customer").Bold();
                        header.Cell().Text("Date").Bold();
                        header.Cell().Text("Status").Bold();
                        header.Cell().Text("Freight").Bold();
                        header.Cell().Text("Total").Bold();
                    });

                    foreach (var order in orders)
                    {
                        table.Cell().Text(order.OrderId.ToString());
                        table.Cell().Text(order.CustomerName ?? "");
                        table.Cell().Text(order.OrderDate?.ToString("yyyy-MM-dd") ?? "");
                        table.Cell().Text(order.ShippedDate.HasValue ? "Delivered" : "In Transit");
                        table.Cell().Text($"${order.Freight:F2}");
                        table.Cell().Text($"${order.TotalAmount:F2}");
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Northwind Traders © 2026 — Generated on ");
                    t.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        });

        return document.GeneratePdf();
    }
}
