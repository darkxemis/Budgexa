namespace Budgexa.Application.Invoices.Commands.CreateInvoice;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Invoices.Queries;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class CreateInvoiceCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
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

        if (dto.BudgetId.HasValue)
        {
            var budgetExists = await db.Budgets
                .AsNoTracking()
                .AnyAsync(b =>
                    b.Id == dto.BudgetId.Value
                    && b.CompanyId == companyId
                    && b.StatusId != StatusIds.Delete,
                    cancellationToken);

            if (!budgetExists)
                throw new AppException(HttpStatusCode.NotFound, ErrorTags.Budget.NotFound, "Budget not found for this company.");
        }

        var numberTaken = await db.Invoices
            .AsNoTracking()
            .AnyAsync(i =>
                i.CompanyId == companyId
                && i.Series == dto.Series
                && i.Number == dto.Number
                && i.StatusId != StatusIds.Delete,
                cancellationToken);

        if (numberTaken)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Invoice.NumberAlreadyExists, "Invoice series + number already exists for this company.");

        var invoice = Invoice.Create(
            companyId,
            dto.CustomerId,
            StatusIds.Invoice.Draft,
            dto.Series,
            dto.Number,
            dto.IssueDate,
            dto.DueDate,
            dto.Currency,
            dto.Notes,
            currentUserId,
            dto.BudgetId);

        if (dto.Lines is not null)
        {
            foreach (var line in dto.Lines)
            {
                invoice.AddLine(
                    line.ItemId,
                    line.SortOrder,
                    line.Description,
                    line.Unit,
                    line.Quantity,
                    line.UnitPrice,
                    line.DiscountPercentage,
                    line.TaxRate,
                    line.WithholdingRate,
                    currentUserId,
                    line.Id);
            }
        }

        await db.Invoices.AddAsync(invoice, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await InvoiceProjections.ProjectByIdAsync(db, invoice.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve created invoice.");
    }
}
