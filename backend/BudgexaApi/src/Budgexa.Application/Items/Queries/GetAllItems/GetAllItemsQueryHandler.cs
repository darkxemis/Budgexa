namespace Budgexa.Application.Items.Queries.GetAllItems;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.DTOs;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllItemsQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllItemsQuery, IEnumerable<ItemDto>>
{
    public async Task<IEnumerable<ItemDto>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await db.Items
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId && i.StatusId != StatusIds.Delete)
            .OrderBy(i => i.Name)
            .Select(i => new ItemDto(
                i.Id,
                i.Sku,
                i.Name,
                i.Description,
                i.Type,
                i.Unit,
                i.UnitPrice,
                i.TaxRate,
                i.Currency,
                i.CompanyId,
                i.Company.Name,
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
