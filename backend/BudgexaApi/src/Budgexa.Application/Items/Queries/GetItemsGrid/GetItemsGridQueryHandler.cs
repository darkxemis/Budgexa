namespace Budgexa.Application.Items.Queries.GetItemsGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Helpers;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Gridify;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetItemsGridQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetItemsGridQuery, GridResponseDto<ItemGridDto>>
{
    public async Task<GridResponseDto<ItemGridDto>> Handle(GetItemsGridQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var gridifyFilters = GridifyHelper.BuildFilterExpression(dto.Filters);
        var gridifySorting = GridifyHelper.BuildSortingExpression(dto.Sorting);

        IQueryable<Item> query = db.Items
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId)
            .Where(i => i.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var search = dto.Search.ToLower();
            query = query.Where(i =>
                (i.Sku != null && i.Sku.ToLower().Contains(search)) ||
                i.Name.ToLower().Contains(search) ||
                (i.Description != null && i.Description.ToLower().Contains(search)) ||
                i.Unit.ToLower().Contains(search) ||
                i.Currency.ToLower().Contains(search));
        }

        var mapper = GetItemGridMapper(languageId);

        if (!string.IsNullOrWhiteSpace(gridifyFilters))
        {
            query = query.ApplyFiltering(gridifyFilters, mapper);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(gridifySorting))
        {
            query = query.ApplyOrdering(gridifySorting, mapper);
        }
        else
        {
            query = query.OrderBy(i => i.Name);
        }

        var projected = query.Select(i => new ItemGridDto(
            i.Id,
            i.Sku,
            i.Name,
            i.Description,
            i.Type,
            i.Unit,
            i.UnitPrice,
            i.TaxRate,
            i.Currency,
            i.StatusId,
            i.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? i.Status.Name,
            i.CreatedAt,
            i.UpdatedAt));

        return await projected.ToGridResponseAsync(dto.Page, dto.PageSize, totalCount, cancellationToken);
    }

    private static IGridifyMapper<Item> GetItemGridMapper(Guid languageId)
    {
        return new GridifyMapper<Item>()
            .GenerateMappings()
            .AddMap("Id", i => i.Id)
            .AddMap("Sku", i => i.Sku!)
            .AddMap("Name", i => i.Name)
            .AddMap("Description", i => i.Description!)
            .AddMap("Type", i => (int)i.Type)
            .AddMap("Unit", i => i.Unit)
            .AddMap("UnitPrice", i => i.UnitPrice)
            .AddMap("TaxRate", i => i.TaxRate)
            .AddMap("Currency", i => i.Currency)
            .AddMap("StatusId", i => i.StatusId)
            .AddMap("StatusName", i => i.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? i.Status.Name)
            .AddMap("CreatedAt", i => i.CreatedAt.Date)
            .AddMap("UpdatedAt", i => i.UpdatedAt.HasValue ? i.UpdatedAt.Value.Date : null);
    }
}
