namespace Budgexa.Application.Invoices.Commands.DeleteInvoice;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteInvoiceCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<DeleteInvoiceCommand>
{
    public async Task Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
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

        invoice.ChangeStatus(StatusIds.Delete, currentUserId);
        await db.SaveChangesAsync(cancellationToken);
    }
}
