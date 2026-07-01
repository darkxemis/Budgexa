namespace Budgexa.Application.Customers.Commands.CreateCustomer;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class CreateCustomerCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var taxIdExists = await db.Customers
            .AsNoTracking()
            .AnyAsync(c =>
                c.CompanyId == companyId
                && c.TaxId == dto.TaxId
                && c.StatusId != StatusIds.Delete,
                cancellationToken);

        if (taxIdExists)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Customer.TaxIdAlreadyExists, "Customer tax id already exists for this company.");

        var newStatus = await db.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Value == (int)BaseStatus.New, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Status.NotFound, "Status 'New' not found.");

        var customer = Customer.Create(
            companyId,
            newStatus.Id,
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

        await db.Customers.AddAsync(customer, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var created = await db.Customers
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

        return created
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve created customer.");
    }
}
