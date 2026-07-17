namespace Budgexa.Application.Invoices.Queries.GetInvoicesGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Helpers;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Gridify;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetInvoicesGridQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetInvoicesGridQuery, GridResponseDto<InvoiceGridDto>>
{
    public async Task<GridResponseDto<InvoiceGridDto>> Handle(GetInvoicesGridQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var gridifyFilters = GridifyHelper.BuildFilterExpression(dto.Filters);
        var gridifySorting = GridifyHelper.BuildSortingExpression(dto.Sorting);

        IQueryable<Invoice> query = db.Invoices
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId)
            .Where(i => i.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var search = dto.Search.ToLower();
            query = query.Where(i =>
                i.Number.ToLower().Contains(search) ||
                i.Series.ToLower().Contains(search) ||
                i.Customer.LegalName.ToLower().Contains(search) ||
                (i.Customer.TradeName != null && i.Customer.TradeName.ToLower().Contains(search)) ||
                i.Customer.TaxId.ToLower().Contains(search) ||
                i.Currency.ToLower().Contains(search));
        }

        var mapper = GetInvoiceGridMapper(languageId);

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
            query = query.OrderByDescending(i => i.IssueDate)
                .ThenBy(i => i.Series)
                .ThenBy(i => i.Number);
        }

        var projected = query.Select(i => new InvoiceGridDto(
            i.Id,
            i.Series,
            i.Number,
            i.IssueDate,
            i.DueDate,
            i.Currency,
            i.Total,
            i.AmountPaid,
            i.Total - i.AmountPaid,
            i.CustomerId,
            i.Customer.LegalName,
            i.StatusId,
            i.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? i.Status.Name,
            i.CreatedAt,
            i.UpdatedAt));

        return await projected.ToGridResponseAsync(dto.Page, dto.PageSize, totalCount, cancellationToken);
    }

    private static IGridifyMapper<Invoice> GetInvoiceGridMapper(Guid languageId)
    {
        return new GridifyMapper<Invoice>()
            .GenerateMappings()
            .AddMap("Id", i => i.Id)
            .AddMap("Series", i => i.Series)
            .AddMap("Number", i => i.Number)
            .AddMap("IssueDate", i => i.IssueDate)
            .AddMap("DueDate", i => i.DueDate)
            .AddMap("Currency", i => i.Currency)
            .AddMap("Total", i => i.Total)
            .AddMap("AmountPaid", i => i.AmountPaid)
            .AddMap("AmountDue", i => i.Total - i.AmountPaid)
            .AddMap("CustomerId", i => i.CustomerId)
            .AddMap("CustomerName", i => i.Customer.LegalName)
            .AddMap("StatusId", i => i.StatusId)
            .AddMap("StatusName", i => i.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? i.Status.Name)
            .AddMap("CreatedAt", i => i.CreatedAt.Date)
            .AddMap("UpdatedAt", i => i.UpdatedAt.HasValue ? i.UpdatedAt.Value.Date : null);
    }
}
