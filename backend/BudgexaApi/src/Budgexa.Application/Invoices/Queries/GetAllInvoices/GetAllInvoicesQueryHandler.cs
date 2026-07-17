namespace Budgexa.Application.Invoices.Queries.GetAllInvoices;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllInvoicesQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllInvoicesQuery, List<InvoiceGridDto>>
{
    public async Task<List<InvoiceGridDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await db.Invoices
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId)
            .Where(i => i.StatusId != StatusIds.Delete)
            .OrderByDescending(i => i.IssueDate)
            .ThenBy(i => i.Series)
            .ThenBy(i => i.Number)
            .Select(i => new InvoiceGridDto(
                i.Id,
                i.Series,
                i.Number,
                i.IssueDate,
                i.DueDate,
                i.Currency,
                i.Total,
                i.AmountPaid,
                i.Total - i.AmountPaid,
                i.CustomerId,
                i.Customer.LegalName,
                i.StatusId,
                i.Status.Translations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Translation)
                    .FirstOrDefault() ?? i.Status.Name,
                i.CreatedAt,
                i.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
