namespace Budgexa.Application.Customers.Queries.GetCustomerById;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetCustomerByIdQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var customer = await db.Customers
            .AsNoTracking()
            .Where(c =>
                c.Id == request.Id
                && c.CompanyId == companyId
                && c.StatusId != StatusIds.Delete)
            .Select(c => new CustomerDto(
                c.Id,
                c.LegalName,
                c.TradeName,
                c.TaxId,
                c.Email,
                c.Phone,
                c.AddressLine,
                c.City,
                c.PostalCode,
                c.Province,
                c.Country,
                c.Notes,
                c.CompanyId,
                c.Company.Name,
                c.StatusId,
                c.Status.Translations
                    .Where(t => t.LanguageId == languageId)
                    .Select(t => t.Translation)
                    .FirstOrDefault() ?? c.Status.Name,
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return customer
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Customer.NotFound, "Customer not found.");
    }
}
