namespace Budgexa.Infrastructure.Services.Pdf.Invoices;

using Budgexa.Application.Invoices.Services;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

internal sealed class InvoicePdfService : IInvoicePdfService
{
    public byte[] GeneratePdf(Invoice invoice, Company company, Customer customer, byte[]? signatureBytes, string languageCode)
    {
        var labels = InvoicePdfLabelsFactory.Create(languageCode);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(50);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => ComposeHeader(header, labels, company, invoice, customer));
                page.Content().Element(content => ComposeContent(content, labels, invoice));
                page.Footer().Element(footer => ComposeFooter(footer, labels, signatureBytes));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(
        IContainer container, InvoicePdfLabels labels, Company company, Invoice invoice, Customer customer)
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

                    right.Item().Text($"# {invoice.Series}-{invoice.Number}")
                        .FontSize(10).SemiBold();

                    right.Item().Text($"{labels.DateLabel}: {invoice.IssueDate:dd/MM/yyyy}")
                        .FontSize(9);

                    right.Item().Text($"{labels.DueDateLabel}: {invoice.DueDate:dd/MM/yyyy}")
                        .FontSize(9);
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

    private static void ComposeContent(IContainer container, InvoicePdfLabels labels, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(65);
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
                        .Text(labels.DiscountColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.TaxColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.WithholdingColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text(labels.TotalColumn).FontColor(Colors.White).FontSize(9).SemiBold();
                });

                var isAlternate = false;
                foreach (var line in invoice.Lines)
                {
                    var bg = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Background(bg).Padding(5).Text(line.Description).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.Quantity.ToString("N2")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.UnitPrice.ToString("N2")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.DiscountPercentage.ToString("N1")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.TaxRate.ToString("N1")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.WithholdingRate.ToString("N1")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(line.Total.ToString("N2")).FontSize(9);

                    isAlternate = !isAlternate;
                }
            });

            column.Item().PaddingTop(20).AlignRight().Width(220).Column(totals =>
            {
                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text(labels.SubTotalLabel).FontSize(10).SemiBold();
                    row.ConstantItem(90).AlignRight().Text(invoice.Subtotal.ToString("N2")).FontSize(10);
                });

                totals.Item().PaddingVertical(3).Row(row =>
                {
                    row.RelativeItem().Text(labels.TaxTotalLabel).FontSize(10).SemiBold();
                    row.ConstantItem(90).AlignRight().Text(invoice.TaxAmount.ToString("N2")).FontSize(10);
                });

                totals.Item().PaddingVertical(3).Row(row =>
                {
                    row.RelativeItem().Text(labels.WithholdingTotalLabel).FontSize(10).SemiBold();
                    row.ConstantItem(90).AlignRight().Text(invoice.WithholdingAmount.ToString("N2")).FontSize(10);
                });

                totals.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Blue.Darken3).PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(labels.GrandTotalLabel).FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                    row.ConstantItem(90).AlignRight().Text(invoice.Total.ToString("N2")).FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                });
            });

            if (invoice.AmountPaid > 0)
            {
                column.Item().PaddingTop(20).Column(payment =>
                {
                    payment.Item().Text(labels.PaymentInfoLabel).FontSize(11).SemiBold();

                    payment.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text(labels.AmountPaidLabel).FontSize(10);
                        row.ConstantItem(90).AlignRight().Text(invoice.AmountPaid.ToString("N2")).FontSize(10);
                    });

                    payment.Item().PaddingVertical(3).Row(row =>
                    {
                        row.RelativeItem().Text(labels.AmountDueLabel).FontSize(10).SemiBold();
                        row.ConstantItem(90).AlignRight().Text(invoice.AmountDue.ToString("N2")).FontSize(10).SemiBold();
                    });

                    if (invoice.PaymentMethod.HasValue)
                    {
                        payment.Item().PaddingVertical(3).Row(row =>
                        {
                            row.RelativeItem().Text(labels.PaymentMethodLabel).FontSize(10);
                            row.ConstantItem(120).AlignRight().Text(GetPaymentMethodLabel(labels, invoice.PaymentMethod.Value)).FontSize(10);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(invoice.PaymentReference))
                    {
                        payment.Item().PaddingVertical(3).Row(row =>
                        {
                            row.RelativeItem().Text(labels.PaymentReferenceLabel).FontSize(10);
                            row.ConstantItem(120).AlignRight().Text(invoice.PaymentReference).FontSize(10);
                        });
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(invoice.Notes))
            {
                column.Item().PaddingTop(20).Column(notes =>
                {
                    notes.Item().Text(labels.NotesLabel).FontSize(11).SemiBold();
                    notes.Item().PaddingTop(5).Text(invoice.Notes).FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container, InvoicePdfLabels labels, byte[]? signatureBytes)
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

    private static string GetPaymentMethodLabel(InvoicePdfLabels labels, PaymentMethod method) => method switch
    {
        PaymentMethod.BankTransfer => labels.PaymentMethodBankTransfer,
        PaymentMethod.Cash => labels.PaymentMethodCash,
        PaymentMethod.Card => labels.PaymentMethodCard,
        PaymentMethod.DirectDebit => labels.PaymentMethodDirectDebit,
        PaymentMethod.Bizum => labels.PaymentMethodBizum,
        PaymentMethod.Other => labels.PaymentMethodOther,
        _ => method.ToString()
    };
}
