namespace Budgexa.Application.Budgets.Queries.GetBudgetsGrid;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Helpers;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Gridify;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetBudgetsGridQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetBudgetsGridQuery, GridResponseDto<BudgetGridDto>>
{
    public async Task<GridResponseDto<BudgetGridDto>> Handle(GetBudgetsGridQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var gridifyFilters = GridifyHelper.BuildFilterExpression(dto.Filters);
        var gridifySorting = GridifyHelper.BuildSortingExpression(dto.Sorting);

        IQueryable<Budget> query = db.Budgets
            .AsNoTracking()
            .Where(b => b.CompanyId == companyId)
            .Where(b => b.StatusId != StatusIds.Delete);

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var search = dto.Search.ToLower();
            query = query.Where(b =>
                b.Number.ToLower().Contains(search) ||
                b.Customer.LegalName.ToLower().Contains(search) ||
                (b.Customer.TradeName != null && b.Customer.TradeName.ToLower().Contains(search)) ||
                b.Customer.TaxId.ToLower().Contains(search) ||
                b.Currency.ToLower().Contains(search));
        }

        var mapper = GetBudgetGridMapper(languageId);

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
            query = query.OrderByDescending(b => b.IssueDate).ThenBy(b => b.Number);
        }

        var projected = query.Select(b => new BudgetGridDto(
            b.Id,
            b.Number,
            b.IssueDate,
            b.ValidUntil,
            b.Currency,
            b.Total,
            b.CustomerId,
            b.Customer.LegalName,
            b.StatusId,
            b.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? b.Status.Name,
            b.CreatedAt,
            b.UpdatedAt));

        return await projected.ToGridResponseAsync(dto.Page, dto.PageSize, totalCount, cancellationToken);
    }

    private static IGridifyMapper<Budget> GetBudgetGridMapper(Guid languageId)
    {
        return new GridifyMapper<Budget>()
            .GenerateMappings()
            .AddMap("Id", b => b.Id)
            .AddMap("Number", b => b.Number)
            .AddMap("IssueDate", b => b.IssueDate)
            .AddMap("ValidUntil", b => b.ValidUntil!)
            .AddMap("Currency", b => b.Currency)
            .AddMap("Total", b => b.Total)
            .AddMap("CustomerId", b => b.CustomerId)
            .AddMap("CustomerName", b => b.Customer.LegalName)
            .AddMap("StatusId", b => b.StatusId)
            .AddMap("StatusName", b => b.Status.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? b.Status.Name)
            .AddMap("CreatedAt", b => b.CreatedAt.Date)
            .AddMap("UpdatedAt", b => b.UpdatedAt.HasValue ? b.UpdatedAt.Value.Date : null);
    }
}
