namespace Budgexa.Application.Customers.Queries.GetAllCustomers;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllCustomersQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
{
    public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await db.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId && c.StatusId != StatusIds.Delete)
            .OrderBy(c => c.LegalName)
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
            .ToListAsync(cancellationToken);
    }
}
