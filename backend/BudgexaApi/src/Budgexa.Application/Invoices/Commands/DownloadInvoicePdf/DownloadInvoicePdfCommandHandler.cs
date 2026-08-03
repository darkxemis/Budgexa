namespace Budgexa.Application.Invoices.Commands.DownloadInvoicePdf;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Services;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class DownloadInvoicePdfCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService,
    IInvoicePdfService pdfService,
    IFileStorageService fileStorage
) : IRequestHandler<DownloadInvoicePdfCommand, DownloadInvoicePdfResult>
{
    public async Task<DownloadInvoicePdfResult> Handle(DownloadInvoicePdfCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var companyId = currentUserService.CompanyId;

        var invoice = await db.Invoices
            .Include(i => i.Company)
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.CompanyId == companyId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Invoice.NotFound, "The requested invoice was not found.");

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        var languageCode = await GetLanguageCodeAsync(db, currentUserService, cancellationToken);

        var signatureBytes = await fileStorage.GetSignatureImageBytesAsync(user.SignatureUrl, cancellationToken);

        var pdfBytes = pdfService.GeneratePdf(
            invoice,
            invoice.Company,
            invoice.Customer,
            signatureBytes,
            languageCode);

        var fileName = $"Invoice_{invoice.Series}_{invoice.Number}_{invoice.IssueDate:yyyyMMdd}.pdf";

        return new DownloadInvoicePdfResult(pdfBytes, fileName);
    }

    private static async Task<string> GetLanguageCodeAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);
        var language = await db.Languages
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == languageId, cancellationToken);

        return language?.Code ?? "en";
    }
}
