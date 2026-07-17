namespace Budgexa.Application.Budgets.Commands.CreateBudget;

using System.Net;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Queries;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class CreateBudgetCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateBudgetCommand, BudgetDto>
{
    public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var customerExists = await db.Customers
            .AsNoTracking()
            .AnyAsync(c =>
                c.Id == dto.CustomerId
                && c.CompanyId == companyId
                && c.StatusId != StatusIds.Delete,
                cancellationToken);

        if (!customerExists)
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Customer.NotFound, "Customer not found for this company.");

        var numberTaken = await db.Budgets
            .AsNoTracking()
            .AnyAsync(b =>
                b.CompanyId == companyId
                && b.Number == dto.Number
                && b.StatusId != StatusIds.Delete,
                cancellationToken);

        if (numberTaken)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Budget.NumberAlreadyExists, "Budget number already exists for this company.");

        var budget = Budget.Create(
            companyId,
            dto.CustomerId,
            StatusIds.Budget.Draft,
            dto.Number,
            dto.IssueDate,
            dto.ValidUntil,
            dto.Currency,
            dto.Notes,
            dto.TermsAndConditions,
            currentUserId);

        if (dto.Lines is not null)
        {
            foreach (var line in dto.Lines)
            {
                budget.AddLine(
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
            }
        }

        await db.Budgets.AddAsync(budget, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await BudgetProjections.ProjectByIdAsync(db, budget.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve created budget.");
    }
}
