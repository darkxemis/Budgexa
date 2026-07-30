namespace Budgexa.Application.Budgets.Commands.DownloadBudgetPdf;

using System.Net;
using Budgexa.Application.Budgets.Services;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class DownloadBudgetPdfCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService,
    IBudgetPdfService pdfService,
    IFileStorageService fileStorage
) : IRequestHandler<DownloadBudgetPdfCommand, DownloadBudgetPdfResult>
{
    public async Task<DownloadBudgetPdfResult> Handle(DownloadBudgetPdfCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var companyId = currentUserService.CompanyId;

        var budget = await db.Budgets
            .Include(b => b.Company)
            .Include(b => b.Customer)
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.CompanyId == companyId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Budget.NotFound, "The requested budget was not found.");

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        var languageCode = await GetLanguageCodeAsync(db, currentUserService, cancellationToken);

        var signatureBytes = await fileStorage.GetSignatureImageBytesAsync(user.SignatureUrl, cancellationToken);

        var pdfBytes = pdfService.GeneratePdf(
            budget,
            budget.Company,
            budget.Customer,
            signatureBytes,
            languageCode);

        var fileName = $"Budget_{budget.Number}_{budget.IssueDate:yyyyMMdd}.pdf";

        return new DownloadBudgetPdfResult(pdfBytes, fileName);
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
