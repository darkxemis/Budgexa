namespace Budgexa.Application.Items.Queries.GetItemById;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetItemByIdQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetItemByIdQuery, ItemDto>
{
    public async Task<ItemDto> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var item = await db.Items
            .AsNoTracking()
            .Where(i =>
                i.Id == request.Id
                && i.CompanyId == companyId
                && i.StatusId != StatusIds.Delete)
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
            .FirstOrDefaultAsync(cancellationToken);

        return item
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Item.NotFound, "Item not found.");
    }
}
