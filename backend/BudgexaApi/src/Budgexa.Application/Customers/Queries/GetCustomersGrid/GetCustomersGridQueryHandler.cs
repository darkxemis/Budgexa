namespace Budgexa.Application.Customers.Queries.GetCustomersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Helpers;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Gridify;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetCustomersGridQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetCustomersGridQuery, GridResponseDto<CustomerGridDto>>
{
    public async Task<GridResponseDto<CustomerGridDto>> Handle(GetCustomersGridQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var gridifyFilters = GridifyHelper.BuildFilterExpression(dto.Filters);
        var gridifySorting = GridifyHelper.BuildSortingExpression(dto.Sorting);

        IQueryable<Customer> query = db.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .Where(c => c.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var search = dto.Search.ToLower();
            query = query.Where(c =>
                c.LegalName.ToLower().Contains(search) ||
                (c.TradeName != null && c.TradeName.ToLower().Contains(search)) ||
                c.TaxId.ToLower().Contains(search) ||
                (c.Email != null && c.Email.ToLower().Contains(search)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(search)) ||
                (c.City != null && c.City.ToLower().Contains(search)) ||
                (c.Country != null && c.Country.ToLower().Contains(search)));
        }

        var mapper = GetCustomerGridMapper(languageId);

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
            query = query.OrderBy(c => c.LegalName);
        }

        var projected = query.Select(c => new CustomerGridDto(
            c.Id,
            c.LegalName,
            c.TradeName,
            c.TaxId,
            c.Email,
            c.Phone,
            c.City,
            c.Country,
            c.StatusId,
            c.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? c.Status.Name,
            c.CreatedAt,
            c.UpdatedAt));

        return await projected.ToGridResponseAsync(dto.Page, dto.PageSize, totalCount, cancellationToken);
    }

    private static IGridifyMapper<Customer> GetCustomerGridMapper(Guid languageId)
    {
        return new GridifyMapper<Customer>()
            .GenerateMappings()
            .AddMap("Id", c => c.Id)
            .AddMap("LegalName", c => c.LegalName)
            .AddMap("TradeName", c => c.TradeName!)
            .AddMap("TaxId", c => c.TaxId)
            .AddMap("Email", c => c.Email!)
            .AddMap("Phone", c => c.Phone!)
            .AddMap("City", c => c.City!)
            .AddMap("Country", c => c.Country!)
            .AddMap("StatusId", c => c.StatusId)
            .AddMap("StatusName", c => c.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? c.Status.Name)
            .AddMap("CreatedAt", c => c.CreatedAt.Date)
            .AddMap("UpdatedAt", c => c.UpdatedAt.HasValue ? c.UpdatedAt.Value.Date : null);
    }
}
