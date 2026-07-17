namespace Budgexa.Application.PublicBudgets.Queries.GetPublicItems;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.PublicBudgets.DTOs;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetPublicItemsQueryHandler(
    IApplicationDbContext db
) : IRequestHandler<GetPublicItemsQuery, List<PublicItemDto>>
{
    public async Task<List<PublicItemDto>> Handle(GetPublicItemsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Items
            .AsNoTracking()
            .Where(i => i.CompanyId == request.CompanyId && i.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(search) ||
                (i.Sku != null && i.Sku.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(i => i.Name)
            .Take(20)
            .Select(i => new PublicItemDto(
                i.Id,
                i.Name,
                i.UnitPrice,
                i.TaxRate,
                i.Unit))
            .ToListAsync(cancellationToken);
    }
}
