using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Northwind.Application.DTOs;

namespace project_API.Northwind.WebAPI.Helpers;

public static class PdfGenerator
{
    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5);
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container.Background("#1E293B").PaddingVertical(5).PaddingHorizontal(5);
    }

    public static byte[] GenerateOrderReport(OrderResponseDto order)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("NORTHWIND TRADERS").Bold().FontSize(24).FontColor("#1E293B");
                        col.Item().Text("Shipping & Logistics Division").FontSize(10).FontColor("#64748B");
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("ORDER REPORT").Bold().FontSize(14).FontColor("#334155");
                        col.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(10).FontColor("#64748B");
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    // Basic Info Section
                    col.Item().Background("#F8FAFC").Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ORDER INFORMATION").Bold().FontSize(10).FontColor("#475569");
                            c.Item().PaddingTop(5).Text($"Order ID: #{order.OrderId}").Bold();
                            c.Item().Text($"Order Date: {order.OrderDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
                            c.Item().Text($"Customer: {order.CustomerName}").Bold();
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("SHIPPING DETAILS").Bold().FontSize(10).FontColor("#475569");
                            c.Item().PaddingTop(5).Text($"Status: {(order.ShippedDate.HasValue ? "DELIVERED" : "IN TRANSIT")}").FontColor(order.ShippedDate.HasValue ? "#059669" : "#D97706").Bold();
                            c.Item().Text($"Ship To: {order.ShipAddress}");
                            c.Item().Text($"{order.ShipCity}, {order.ShipCountry}");
                        });
                    });

                    col.Item().PaddingTop(20).Text("ITEMIZED DETAILS").Bold().FontSize(12).FontColor("#1E293B");

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("#").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).Text("Product").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Qty").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Price").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Disc.").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                        });

                        int i = 1;
                        foreach (var item in order.LineItems)
                        {
                            var subtotal = item.UnitPrice * item.Quantity * (1 - (decimal)item.Discount);
                            
                            IContainer rowStyle(IContainer c) => i % 2 == 0 ? c.Background("#F1F5F9") : c;

                            table.Cell().Element(CellStyle).Element(rowStyle).Text(i++.ToString());
                            table.Cell().Element(CellStyle).Element(rowStyle).Text(item.ProductId.ToString());
                            table.Cell().Element(CellStyle).Element(rowStyle).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().Element(CellStyle).Element(rowStyle).AlignRight().Text($"${item.UnitPrice:N2}");
                            table.Cell().Element(CellStyle).Element(rowStyle).AlignRight().Text($"{item.Discount:P0}");
                            table.Cell().Element(CellStyle).Element(rowStyle).AlignRight().Text($"${subtotal:N2}").Bold();
                        }
                    });

                    // Totals
                    col.Item().PaddingTop(15).AlignRight().Column(c =>
                    {
                        c.Item().Text($"Freight: ${order.Freight ?? 0:N2}").FontSize(10).FontColor("#64748B");
                        c.Item().PaddingTop(2).Text($"Order Total: ${order.TotalAmount:N2}").Bold().FontSize(14).FontColor("#1E293B");
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generated by Northwind Intelligence System • Page ").FontColor("#94A3B8");
                    t.CurrentPageNumber().FontColor("#94A3B8");
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
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Verdana));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("NORTHWIND TRADERS").Bold().FontSize(20).FontColor("#1E293B");
                        col.Item().Text("Executive Orders Summary").FontSize(12).FontColor("#64748B");
                    });

                    row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("MMMM dd, yyyy")).FontColor("#64748B");
                });

                page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("ID").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).Text("Customer").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).Text("Date").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).Text("Status").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Freight").FontColor(Colors.White).Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                        });

                        int i = 0;
                        foreach (var order in orders)
                        {
                            IContainer rowStyle(IContainer c) => i % 2 == 0 ? c.Background("#F8FAFC") : c;
                            i++;

                            table.Cell().Element(CellStyle).Element(rowStyle).Text(order.OrderId.ToString());
                            table.Cell().Element(CellStyle).Element(rowStyle).Text(order.CustomerName ?? "N/A");
                            table.Cell().Element(CellStyle).Element(rowStyle).Text(order.OrderDate?.ToString("yyyy-MM-dd") ?? "");
                            table.Cell().Element(CellStyle).Element(rowStyle).Text(order.ShippedDate.HasValue ? "Delivered" : "In Transit")
                                .FontColor(order.ShippedDate.HasValue ? "#059669" : "#D97706");
                            table.Cell().Element(CellStyle).Element(rowStyle).AlignRight().Text($"${order.Freight ?? 0:N2}");
                            table.Cell().Element(CellStyle).Element(rowStyle).AlignRight().Text($"${order.TotalAmount:N2}").Bold();
                        }
                    });

                    col.Item().PaddingTop(10).AlignRight().Text(t =>
                    {
                        t.Span("Grand Total: ").FontSize(12).Bold();
                        t.Span($"${orders.Sum(o => o.TotalAmount):N2}").FontSize(12).Bold().FontColor("#1E293B");
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Confidential Document • Northwind Shipping Logistics").FontColor("#94A3B8");
                });
            });
        });

        return document.GeneratePdf();
    }
}
