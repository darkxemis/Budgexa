namespace Budgexa.Infrastructure.Services.Pdf.Common;

internal static class PdfLabelsFactory
{
    public static PdfLabels Create(string languageCode) => languageCode switch
    {
        "es" => new PdfLabels(
            Title: "Presupuesto",
            CustomerLabel: "Cliente",
            PhoneLabel: "Teléfono",
            EmailLabel: "Email",
            DateLabel: "Fecha",
            ItemColumn: "Descripción",
            QuantityColumn: "Cantidad",
            UnitPriceColumn: "Precio Unit.",
            TaxColumn: "IVA (%)",
            SubTotalColumn: "Subtotal",
            SubTotalLabel: "Subtotal",
            TaxTotalLabel: "Total Impuestos",
            GrandTotalLabel: "Total General",
            FooterText: "Gracias por su confianza. Este presupuesto ha sido generado automáticamente.",
            SignatureLabel: "Firma"),

        "de" => new PdfLabels(
            Title: "Kostenvoranschlag",
            CustomerLabel: "Kunde",
            PhoneLabel: "Telefon",
            EmailLabel: "E-Mail",
            DateLabel: "Datum",
            ItemColumn: "Beschreibung",
            QuantityColumn: "Menge",
            UnitPriceColumn: "Einzelpreis",
            TaxColumn: "MwSt. (%)",
            SubTotalColumn: "Zwischensumme",
            SubTotalLabel: "Zwischensumme",
            TaxTotalLabel: "Steuern Gesamt",
            GrandTotalLabel: "Gesamtbetrag",
            FooterText: "Vielen Dank für Ihr Vertrauen. Dieses Angebot wurde automatisch erstellt.",
            SignatureLabel: "Unterschrift"),

        "hr" => new PdfLabels(
            Title: "Ponuda",
            CustomerLabel: "Klijent",
            PhoneLabel: "Telefon",
            EmailLabel: "Email",
            DateLabel: "Datum",
            ItemColumn: "Opis",
            QuantityColumn: "Količina",
            UnitPriceColumn: "Jed. cijena",
            TaxColumn: "PDV (%)",
            SubTotalColumn: "Podzbroj",
            SubTotalLabel: "Podzbroj",
            TaxTotalLabel: "Ukupni porez",
            GrandTotalLabel: "Ukupno",
            FooterText: "Hvala na povjerenju. Ova ponuda je automatski generirana.",
            SignatureLabel: "Potpis"),

        _ => new PdfLabels(
            Title: "Budget",
            CustomerLabel: "Customer",
            PhoneLabel: "Phone",
            EmailLabel: "Email",
            DateLabel: "Date",
            ItemColumn: "Description",
            QuantityColumn: "Qty",
            UnitPriceColumn: "Unit Price",
            TaxColumn: "Tax (%)",
            SubTotalColumn: "Subtotal",
            SubTotalLabel: "Subtotal",
            TaxTotalLabel: "Tax Total",
            GrandTotalLabel: "Grand Total",
            FooterText: "Thank you for your trust. This budget has been automatically generated.",
            SignatureLabel: "Signature")
    };
}
