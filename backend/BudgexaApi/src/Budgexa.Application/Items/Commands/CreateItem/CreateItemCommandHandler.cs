namespace Budgexa.Application.Items.Commands.CreateItem;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class CreateItemCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateItemCommand, ItemDto>
{
    public async Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var skuExists = await db.Items
                .AsNoTracking()
                .AnyAsync(i =>
                    i.CompanyId == companyId
                    && i.Sku == dto.Sku
                    && i.StatusId != StatusIds.Delete,
                    cancellationToken);

            if (skuExists)
                throw new AppException(HttpStatusCode.Conflict, ErrorTags.Item.SkuAlreadyExists, "Item SKU already exists for this company.");
        }

        var newStatus = await db.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Value == (int)BaseStatus.New, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Status.NotFound, "Status 'New' not found.");

        var item = Item.Create(
            companyId,
            newStatus.Id,
            dto.Sku,
            dto.Name,
            dto.Description,
            dto.Type,
            dto.Unit,
            dto.UnitPrice,
            dto.TaxRate,
            dto.Currency,
            currentUserId);

        await db.Items.AddAsync(item, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var created = await db.Items
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

        return created
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve created item.");
    }
}
