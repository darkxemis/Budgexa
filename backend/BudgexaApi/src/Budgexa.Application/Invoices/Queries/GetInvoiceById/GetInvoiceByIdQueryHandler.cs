namespace Budgexa.Application.Invoices.Queries.GetInvoiceById;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetInvoiceByIdQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;

        var exists = await db.Invoices
            .AsNoTracking()
            .AnyAsync(i =>
                i.Id == request.Id
                && i.CompanyId == companyId
                && i.StatusId != StatusIds.Delete,
                cancellationToken);

        if (!exists)
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Invoice.NotFound, "Invoice not found.");

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await InvoiceProjections.ProjectByIdAsync(db, request.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Invoice.NotFound, "Invoice not found.");
    }
}
