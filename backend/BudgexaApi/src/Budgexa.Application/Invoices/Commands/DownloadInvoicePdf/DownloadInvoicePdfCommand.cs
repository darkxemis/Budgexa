namespace Budgexa.Application.Invoices.Commands.DownloadInvoicePdf;

using MediatR;

public sealed record DownloadInvoicePdfCommand(Guid InvoiceId) : IRequest<DownloadInvoicePdfResult>;

public sealed record DownloadInvoicePdfResult(byte[] PdfBytes, string FileName);
