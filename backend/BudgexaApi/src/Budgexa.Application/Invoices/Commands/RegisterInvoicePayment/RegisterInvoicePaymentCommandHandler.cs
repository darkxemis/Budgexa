namespace Budgexa.Application.Invoices.Commands.RegisterInvoicePayment;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Invoices.Queries;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class RegisterInvoicePaymentCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<RegisterInvoicePaymentCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(RegisterInvoicePaymentCommand request, CancellationToken cancellationToken)
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

        if (invoice.IsFullyPaid)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Invoice.AlreadyPaid, "Invoice is already fully paid.");

        try
        {
            invoice.RegisterPayment(request.Dto.Amount, request.Dto.Method, request.Dto.Reference, currentUserId);
        }
        catch (ArgumentException ex)
        {
            throw new AppException(HttpStatusCode.BadRequest, ErrorTags.Validation.Failed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Invoice.AlreadyPaid, ex.Message);
        }

        if (invoice.IsFullyPaid)
        {
            invoice.ChangeStatus(StatusIds.Invoice.Paid, currentUserId);
        }
        else
        {
            invoice.ChangeStatus(StatusIds.Invoice.PartiallyPaid, currentUserId);
        }

        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await InvoiceProjections.ProjectByIdAsync(db, invoice.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve invoice after payment.");
    }
}
