namespace Budgexa.Infrastructure.Services.Pdf;

internal sealed record PdfLabels(
    string Title,
    string CustomerLabel,
    string DateLabel,
    string ItemColumn,
    string QuantityColumn,
    string UnitPriceColumn,
    string TaxColumn,
    string SubTotalColumn,
    string SubTotalLabel,
    string TaxTotalLabel,
    string GrandTotalLabel,
    string FooterText);
