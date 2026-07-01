namespace Budgexa.Application.Budgets.Queries.GetAllBudgets;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllBudgetsQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllBudgetsQuery, IEnumerable<BudgetGridDto>>
{
    public async Task<IEnumerable<BudgetGridDto>> Handle(GetAllBudgetsQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await db.Budgets
            .AsNoTracking()
            .Where(b => b.CompanyId == companyId && b.StatusId != StatusIds.Delete)
            .OrderByDescending(b => b.IssueDate)
            .ThenBy(b => b.Number)
            .Select(b => new BudgetGridDto(
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
                b.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
