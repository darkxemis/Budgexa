namespace Budgexa.Application.Invoices.Services;

using Budgexa.Domain.Entities;

public interface IInvoicePdfService
{
    byte[] GeneratePdf(Invoice invoice, Company company, Customer customer, byte[]? signatureBytes, string languageCode);
}
