namespace Budgexa.Application.Items.Queries.GetItemsForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetItemsForSelectorQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetItemsForSelectorQuery, List<SelectorDto>>
{
    public async Task<List<SelectorDto>> Handle(GetItemsForSelectorQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;

        var query = db.Items
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId && i.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(search) ||
                (i.Sku != null && i.Sku.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(i => i.Name)
            .Select(i => new SelectorDto(i.Id, i.Name))
            .ToListAsync(cancellationToken);
    }
}
