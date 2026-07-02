namespace Budgexa.Application.PublicBudgets.Commands.ConfirmAndDownload;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.PublicBudgets.Services;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class ConfirmAndDownloadCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService,
    IPublicBudgetPdfService pdfService)
    : IRequestHandler<ConfirmAndDownloadCommand, ConfirmAndDownloadResult>
{
    public async Task<ConfirmAndDownloadResult> Handle(
        ConfirmAndDownloadCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;

        var company = await db.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.CompanyId, cancellationToken);

        if (company is null)
        {
            throw new AppException(
                HttpStatusCode.NotFound,
                ErrorTags.PublicBudget.CompanyNotFound,
                "Company not found.");
        }

        if (!company.IsContractValid())
        {
            throw new AppException(
                HttpStatusCode.Forbidden,
                ErrorTags.PublicBudget.CompanyContractExpired,
                "Company contract has expired.");
        }

        var requestedItemIds = dto.Lines.Select(l => l.ItemId).ToList();

        var items = await db.Items
            .AsNoTracking()
            .Where(i => requestedItemIds.Contains(i.Id)
                && i.CompanyId == dto.CompanyId
                && i.StatusId != StatusIds.Delete)
            .ToListAsync(cancellationToken);

        var foundItemIds = items.Select(i => i.Id).ToHashSet();
        var missingIds = requestedItemIds.Where(id => !foundItemIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            throw new AppException(
                HttpStatusCode.BadRequest,
                ErrorTags.PublicBudget.ItemNotFound,
                "One or more items were not found or do not belong to this company.",
                missingIds.ToDictionary(
                    id => id.ToString(),
                    _ => "Item not found or inactive for this company."));
        }

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);
        if (languageId == Guid.Empty)
            languageId = LanguageIds.English;

        var languageCode = await db.Languages
            .AsNoTracking()
            .Where(l => l.Id == languageId)
            .Select(l => l.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? LanguageIds.EnglishCode;

        var budget = PublicBudget.Create(
            dto.CompanyId,
            languageId,
            dto.CustomerFirstName,
            dto.CustomerLastName);

        var lines = dto.Lines.Select(lineDto =>
        {
            var item = items.First(i => i.Id == lineDto.ItemId);
            return PublicBudgetLine.Create(
                item.Id,
                item.Name,
                lineDto.Quantity,
                item.UnitPrice,
                item.TaxRate);
        }).ToList();

        budget.AddLines(lines);

        db.PublicBudgets.Add(budget);
        await db.SaveChangesAsync(cancellationToken);

        var pdfBytes = pdfService.GeneratePdf(budget, company, languageCode);
        var fileName = $"Budget_{budget.Id:N}.pdf";

        return new ConfirmAndDownloadResult(pdfBytes, fileName);
    }
}
