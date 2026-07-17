namespace Budgexa.Application.Invoices.Queries.GetInvoicesForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetInvoicesForSelectorQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetInvoicesForSelectorQuery, List<SelectorDto>>
{
    private const int MaxResults = 50;

    public async Task<List<SelectorDto>> Handle(GetInvoicesForSelectorQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;

        var query = db.Invoices
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId)
            .Where(i => i.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(i =>
                i.Number.ToLower().Contains(search) ||
                i.Series.ToLower().Contains(search) ||
                i.Customer.LegalName.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(i => i.IssueDate)
            .ThenBy(i => i.Number)
            .Take(MaxResults)
            .Select(i => new SelectorDto(i.Id, i.Series + "-" + i.Number + " / " + i.Customer.LegalName))
            .ToListAsync(cancellationToken);
    }
}
