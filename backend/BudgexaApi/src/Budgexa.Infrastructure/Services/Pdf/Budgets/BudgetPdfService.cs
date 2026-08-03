namespace Budgexa.Infrastructure.Services.Pdf.Budgets;

using Budgexa.Application.Budgets.Services;
using Budgexa.Domain.Entities;
using Budgexa.Infrastructure.Services.Pdf.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

internal sealed class BudgetPdfService : IBudgetPdfService
{
    public byte[] GeneratePdf(Budget budget, Company company, Customer customer, byte[]? signatureBytes, string languageCode)
    {
        var labels = PdfLabelsFactory.Create(languageCode);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(50);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => ComposeHeader(header, labels, company, budget, customer));
                page.Content().Element(content => ComposeContent(content, labels, budget));
                page.Footer().Element(footer => ComposeFooter(footer, labels, signatureBytes));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(
        IContainer container, PdfLabels labels, Company company, Budget budget, Customer customer)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text(company.Name)
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);

                    if (!string.IsNullOrWhiteSpace(company.Description))
                    {
                        left.Item().Text(company.Description)
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(company.Phone))
                    {
                        left.Item().Text($"{labels.PhoneLabel}: {company.Phone}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(company.Email))
                    {
                        left.Item().Text($"{labels.EmailLabel}: {company.Email}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                });

                row.ConstantItem(180).AlignRight().Column(right =>
                {
                    right.Item().Text(labels.Title)
                        .FontSize(22).Bold().FontColor(Colors.Blue.Darken3);

                    right.Item().Text($"# {budget.Number}")
                        .FontSize(10).SemiBold();

                    right.Item().Text($"{labels.DateLabel}: {budget.IssueDate:dd/MM/yyyy}")
                        .FontSize(9);

                    if (budget.ValidUntil.HasValue)
                    {
                        right.Item().Text($"Valid until: {budget.ValidUntil:dd/MM/yyyy}")
                            .FontSize(9);
                    }
                });
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(customerCol =>
                {
                    customerCol.Item().Text(labels.CustomerLabel)
                        .FontSize(11).SemiBold();

                    customerCol.Item().Text(customer.LegalName)
                        .FontSize(10);

                    if (!string.IsNullOrWhiteSpace(customer.TaxId))
                    {
                        customerCol.Item().Text($"Tax ID: {customer.TaxId}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(customer.Phone))
                    {
                        customerCol.Item().Text($"{labels.PhoneLabel}: {customer.Phone}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(customer.Email))
                    {
                        customerCol.Item().Text($"{labels.EmailLabel}: {customer.Email}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(customer.AddressLine))
                    {
                        var address = customer.AddressLine;
                        if (!string.IsNullOrWhiteSpace(customer.City))
                            address += $", {customer.City}";
                        if (!string.IsNullOrWhiteSpace(customer.Province))
                            address += $", {customer.Province}";
                        if (!string.IsNullOrWhiteSpace(customer.PostalCode))
                            address += $" {customer.PostalCode}";
                        if (!string.IsNullOrWhiteSpace(customer.Country))
                            address += $", {customer.Country}";
                        customerCol.Item().Text(address)
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                });
            });

            column.Item().PaddingTop(15);
        });
    }

    private static void ComposeContent(IContainer container, PdfLabels labels, Budget budget)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.ItemColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.QuantityColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.UnitPriceColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Disc. %").FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.TaxColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.SubTotalColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                });

                var isAlternate = false;
                foreach (var line in budget.Lines)
                {
                    var bg = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Background(bg).Padding(5).Text(line.Description).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.Quantity.ToString("N2")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.UnitPrice.ToString("N2")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.DiscountPercentage.ToString("N1")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.TaxRate.ToString("N1")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.Total.ToString("N2")).FontSize(9);

                    isAlternate = !isAlternate;
                }
            });

            column.Item().PaddingTop(20).AlignRight().Width(220).Column(totals =>
            {
                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text(labels.SubTotalLabel).FontSize(10).SemiBold();
                    row.ConstantItem(90).AlignRight().Text(budget.Subtotal.ToString("N2")).FontSize(10);
                });

                totals.Item().PaddingVertical(3).Row(row =>
                {
                    row.RelativeItem().Text(labels.TaxTotalLabel).FontSize(10).SemiBold();
                    row.ConstantItem(90).AlignRight().Text(budget.TaxAmount.ToString("N2")).FontSize(10);
                });

                totals.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Blue.Darken3).PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(labels.GrandTotalLabel).FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                    row.ConstantItem(90).AlignRight().Text(budget.Total.ToString("N2")).FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                });
            });

            if (!string.IsNullOrWhiteSpace(budget.Notes))
            {
                column.Item().PaddingTop(20).Column(notes =>
                {
                    notes.Item().Text("Notes").FontSize(11).SemiBold();
                    notes.Item().PaddingTop(5).Text(budget.Notes).FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            }

            if (!string.IsNullOrWhiteSpace(budget.TermsAndConditions))
            {
                column.Item().PaddingTop(15).Column(terms =>
                {
                    terms.Item().Text("Terms & Conditions").FontSize(11).SemiBold();
                    terms.Item().PaddingTop(5).Text(budget.TermsAndConditions).FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container, PdfLabels labels, byte[]? signatureBytes)
    {
        container.Column(column =>
        {
            if (signatureBytes is { Length: > 0 })
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(sig =>
                    {
                        sig.Item().Text(labels.SignatureLabel).FontSize(9).SemiBold().FontColor(Colors.Grey.Medium);
                        sig.Item().PaddingTop(4).Width(150).Height(50).Image(signatureBytes);
                    });
                });

                column.Item().PaddingVertical(5);
            }

            column.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            column.Item().Row(row =>
            {
                row.RelativeItem().Text(labels.FooterText)
                    .FontSize(8).FontColor(Colors.Grey.Medium);

                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }
}
