namespace Budgexa.Application.Budgets.Queries;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Read-model projections for <see cref="Domain.Entities.Budget"/> shared by
/// command and query handlers. Kept as a static helper to avoid coupling
/// handlers to each other and to eliminate duplicated <c>Select</c> expressions.
/// </summary>
internal static class BudgetProjections
{
    /// <summary>
    /// Projects a single budget aggregate (with its lines) into a <see cref="BudgetDto"/>.
    /// Uses <c>AsNoTracking</c> because the result is read-only.
    /// </summary>
    public static Task<BudgetDto?> ProjectByIdAsync(
        IApplicationDbContext db,
        Guid id,
        Guid languageId,
        CancellationToken cancellationToken)
    {
        return db.Budgets
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BudgetDto(
                b.Id,
                b.Number,
                b.IssueDate,
                b.ValidUntil,
                b.Currency,
                b.Subtotal,
                b.TaxAmount,
                b.Total,
                b.Notes,
                b.TermsAndConditions,
                b.CompanyId,
                b.Company.Name,
                b.CustomerId,
                b.Customer.LegalName,
                b.StatusId,
                b.Status.Translations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Translation)
                    .FirstOrDefault() ?? b.Status.Name,
                b.Lines
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new BudgetLineDto(
                        l.Id,
                        l.ItemId,
                        l.SortOrder,
                        l.Description,
                        l.Unit,
                        l.Quantity,
                        l.UnitPrice,
                        l.DiscountPercentage,
                        l.TaxRate,
                        l.Subtotal,
                        l.TaxAmount,
                        l.Total))
                    .ToList(),
                b.CreatedAt,
                b.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
