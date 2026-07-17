namespace Budgexa.Application.Invoices.Commands.UpdateInvoice;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Invoices.Queries;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateInvoiceCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<UpdateInvoiceCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var invoice = await db.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i =>
                i.Id == request.Id
                && i.CompanyId == companyId
                && i.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Invoice.NotFound, "Invoice not found.");

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

        if (!string.Equals(invoice.Series, dto.Series, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(invoice.Number, dto.Number, StringComparison.OrdinalIgnoreCase))
        {
            var numberTaken = await db.Invoices
                .AsNoTracking()
                .AnyAsync(i =>
                    i.CompanyId == companyId
                    && i.Id != invoice.Id
                    && i.Series == dto.Series
                    && i.Number == dto.Number
                    && i.StatusId != StatusIds.Delete,
                    cancellationToken);

            if (numberTaken)
                throw new AppException(HttpStatusCode.Conflict, ErrorTags.Invoice.NumberAlreadyExists, "Invoice series + number already exists for this company.");
        }

        invoice.UpdateHeader(
            dto.CustomerId,
            dto.Series,
            dto.Number,
            dto.IssueDate,
            dto.DueDate,
            dto.Currency,
            dto.Notes,
            currentUserId);

        var incomingLines = dto.Lines ?? new();
        var incomingIds = incomingLines.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();

        var existingIds = invoice.Lines.Select(l => l.Id).ToList();
        foreach (var existingId in existingIds)
        {
            if (!incomingIds.Contains(existingId))
                invoice.RemoveLine(existingId, currentUserId);
        }

        foreach (var line in incomingLines)
        {
            if (line.Id.HasValue && invoice.Lines.Any(l => l.Id == line.Id.Value))
            {
                invoice.UpdateLine(
                    line.Id.Value,
                    line.ItemId,
                    line.SortOrder,
                    line.Description,
                    line.Unit,
                    line.Quantity,
                    line.UnitPrice,
                    line.DiscountPercentage,
                    line.TaxRate,
                    line.WithholdingRate,
                    currentUserId);
            }
            else
            {
                var newLine = invoice.AddLine(
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

                await db.InvoiceLines.AddAsync(newLine, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await InvoiceProjections.ProjectByIdAsync(db, invoice.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated invoice.");
    }
}
