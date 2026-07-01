namespace Budgexa.Application.Customers.Commands.UpdateCustomer;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateCustomerCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var customer = await db.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == request.Id
                && c.CompanyId == companyId
                && c.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Customer.NotFound, "Customer not found.");

        var dto = request.Dto;

        if (!string.Equals(customer.TaxId, dto.TaxId, StringComparison.OrdinalIgnoreCase))
        {
            var taxIdTaken = await db.Customers
                .AsNoTracking()
                .AnyAsync(c =>
                    c.CompanyId == companyId
                    && c.Id != customer.Id
                    && c.TaxId == dto.TaxId
                    && c.StatusId != StatusIds.Delete,
                    cancellationToken);

            if (taxIdTaken)
                throw new AppException(HttpStatusCode.Conflict, ErrorTags.Customer.TaxIdAlreadyExists, "Customer tax id already exists for this company.");
        }

        customer.Update(
            dto.LegalName,
            dto.TradeName,
            dto.TaxId,
            dto.Email,
            dto.Phone,
            dto.AddressLine,
            dto.City,
            dto.PostalCode,
            dto.Province,
            dto.Country,
            dto.Notes,
            currentUserId);

        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var updated = await db.Customers
            .AsNoTracking()
            .Where(c => c.Id == customer.Id)
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

        return updated
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated customer.");
    }
}
