namespace Budgexa.Application.Customers.Queries.GetCustomersForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetCustomersForSelectorQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetCustomersForSelectorQuery, List<SelectorDto>>
{
    public async Task<List<SelectorDto>> Handle(GetCustomersForSelectorQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;

        var query = db.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId && c.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(c =>
                c.LegalName.ToLower().Contains(search) ||
                (c.TradeName != null && c.TradeName.ToLower().Contains(search)) ||
                c.TaxId.ToLower().Contains(search));
        }

        return await query
            .OrderBy(c => c.LegalName)
            .Select(c => new SelectorDto(c.Id, c.LegalName))
            .ToListAsync(cancellationToken);
    }
}
