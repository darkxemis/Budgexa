namespace Budgexa.Application.Invoices.Queries;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Read-model projections for <see cref="Domain.Entities.Invoice"/> shared by
/// command and query handlers. Kept as a static helper to avoid coupling
/// handlers to each other and to eliminate duplicated <c>Select</c> expressions.
/// </summary>
internal static class InvoiceProjections
{
    /// <summary>
    /// Projects a single invoice aggregate (with its lines) into an <see cref="InvoiceDto"/>.
    /// Uses <c>AsNoTracking</c> because the result is read-only.
    /// </summary>
    public static Task<InvoiceDto?> ProjectByIdAsync(
        IApplicationDbContext db,
        Guid id,
        Guid languageId,
        CancellationToken cancellationToken)
    {
        return db.Invoices
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InvoiceDto(
                i.Id,
                i.Series,
                i.Number,
                i.IssueDate,
                i.DueDate,
                i.Currency,
                i.Subtotal,
                i.TaxAmount,
                i.WithholdingAmount,
                i.Total,
                i.AmountPaid,
                i.Total - i.AmountPaid,
                i.AmountPaid >= i.Total,
                i.PaymentMethod,
                i.PaymentReference,
                i.Notes,
                i.CompanyId,
                i.Company.Name,
                i.CustomerId,
                i.Customer.LegalName,
                i.BudgetId,
                i.Budget != null ? i.Budget.Number : null,
                i.StatusId,
                i.Status.Translations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Translation)
                    .FirstOrDefault() ?? i.Status.Name,
                i.Lines
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new InvoiceLineDto(
                        l.Id,
                        l.ItemId,
                        l.SortOrder,
                        l.Description,
                        l.Unit,
                        l.Quantity,
                        l.UnitPrice,
                        l.DiscountPercentage,
                        l.TaxRate,
                        l.WithholdingRate,
                        l.Subtotal,
                        l.TaxAmount,
                        l.WithholdingAmount,
                        l.Total))
                    .ToList(),
                i.CreatedAt,
                i.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
