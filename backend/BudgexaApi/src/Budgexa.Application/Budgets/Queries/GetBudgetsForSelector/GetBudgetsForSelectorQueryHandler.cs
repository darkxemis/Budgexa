namespace Budgexa.Application.Budgets.Queries.GetBudgetsForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetBudgetsForSelectorQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetBudgetsForSelectorQuery, List<SelectorDto>>
{
    public async Task<List<SelectorDto>> Handle(GetBudgetsForSelectorQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;

        var query = db.Budgets
            .AsNoTracking()
            .Where(b => b.CompanyId == companyId && b.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(b =>
                b.Number.ToLower().Contains(search) ||
                b.Customer.LegalName.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(b => b.IssueDate)
            .Select(b => new SelectorDto(b.Id, b.Number))
            .ToListAsync(cancellationToken);
    }
}
