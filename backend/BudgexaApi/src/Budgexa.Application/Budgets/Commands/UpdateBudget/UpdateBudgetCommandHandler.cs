namespace Budgexa.Application.Budgets.Commands.UpdateBudget;

using System.Net;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Queries;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateBudgetCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<UpdateBudgetCommand, BudgetDto>
{
    public async Task<BudgetDto> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var budget = await db.Budgets
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b =>
                b.Id == request.Id
                && b.CompanyId == companyId
                && b.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Budget.NotFound, "Budget not found.");

        var dto = request.Dto;

        var customerExists = await db.Customers
            .AsNoTracking()
            .AnyAsync(c =>
                c.Id == dto.CustomerId
                && c.CompanyId == companyId
                && c.StatusId != StatusIds.Delete,
                cancellationToken);

        if (!customerExists)
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Customer.NotFound, "Customer not found for this company.");

        if (!string.Equals(budget.Number, dto.Number, StringComparison.OrdinalIgnoreCase))
        {
            var numberTaken = await db.Budgets
                .AsNoTracking()
                .AnyAsync(b =>
                    b.CompanyId == companyId
                    && b.Id != budget.Id
                    && b.Number == dto.Number
                    && b.StatusId != StatusIds.Delete,
                    cancellationToken);

            if (numberTaken)
                throw new AppException(HttpStatusCode.Conflict, ErrorTags.Budget.NumberAlreadyExists, "Budget number already exists for this company.");
        }

        budget.UpdateHeader(
            dto.CustomerId,
            dto.Number,
            dto.IssueDate,
            dto.ValidUntil,
            dto.Currency,
            dto.Notes,
            dto.TermsAndConditions,
            currentUserId);

        var incomingLines = dto.Lines ?? new();
        var incomingIds = incomingLines.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();

        var existingIds = budget.Lines.Select(l => l.Id).ToList();
        foreach (var existingId in existingIds)
        {
            if (!incomingIds.Contains(existingId))
                budget.RemoveLine(existingId, currentUserId);
        }

        foreach (var line in incomingLines)
        {
            if (line.Id.HasValue && budget.Lines.Any(l => l.Id == line.Id.Value))
            {
                budget.UpdateLine(
                    line.Id.Value,
                    line.ItemId,
                    line.SortOrder,
                    line.Description,
                    line.Unit,
                    line.Quantity,
                    line.UnitPrice,
                    line.DiscountPercentage,
                    line.TaxRate,
                    currentUserId);
            }
            else
            {
                var newLine = budget.AddLine(
                    line.ItemId,
                    line.SortOrder,
                    line.Description,
                    line.Unit,
                    line.Quantity,
                    line.UnitPrice,
                    line.DiscountPercentage,
                    line.TaxRate,
                    currentUserId,
                    line.Id);

                await db.BudgetLines.AddAsync(newLine, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await BudgetProjections.ProjectByIdAsync(db, budget.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated budget.");
    }
}
