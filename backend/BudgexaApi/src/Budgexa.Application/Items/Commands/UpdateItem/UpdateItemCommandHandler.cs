namespace Budgexa.Application.Items.Commands.UpdateItem;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateItemCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<UpdateItemCommand, ItemDto>
{
    public async Task<ItemDto> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var item = await db.Items
            .FirstOrDefaultAsync(i =>
                i.Id == request.Id
                && i.CompanyId == companyId
                && i.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Item.NotFound, "Item not found.");

        var dto = request.Dto;

        if (!string.IsNullOrWhiteSpace(dto.Sku) && !string.Equals(item.Sku, dto.Sku, StringComparison.OrdinalIgnoreCase))
        {
            var skuTaken = await db.Items
                .AsNoTracking()
                .AnyAsync(i =>
                    i.CompanyId == companyId
                    && i.Id != item.Id
                    && i.Sku == dto.Sku
                    && i.StatusId != StatusIds.Delete,
                    cancellationToken);

            if (skuTaken)
                throw new AppException(HttpStatusCode.Conflict, ErrorTags.Item.SkuAlreadyExists, "Item SKU already exists for this company.");
        }

        item.Update(
            dto.Sku,
            dto.Name,
            dto.Description,
            dto.Type,
            dto.Unit,
            dto.UnitPrice,
            dto.TaxRate,
            dto.Currency,
            currentUserId);

        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var updated = await db.Items
            .AsNoTracking()
            .Where(i => i.Id == item.Id)
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

        return updated
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated item.");
    }
}
