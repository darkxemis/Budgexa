namespace Budgexa.Application.Invoices.Commands.ChangeInvoiceStatus;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Invoices.Queries;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class ChangeInvoiceStatusCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<ChangeInvoiceStatusCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(ChangeInvoiceStatusCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var invoice = await db.Invoices
            .FirstOrDefaultAsync(i =>
                i.Id == request.Id
                && i.CompanyId == companyId
                && i.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Invoice.NotFound, "Invoice not found.");

        var statusExists = await db.Statuses
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.Dto.StatusId, cancellationToken);

        if (!statusExists)
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Status.NotFound, "Status not found.");

        invoice.ChangeStatus(request.Dto.StatusId, currentUserId);
        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await InvoiceProjections.ProjectByIdAsync(db, invoice.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve invoice after status change.");
    }
}
