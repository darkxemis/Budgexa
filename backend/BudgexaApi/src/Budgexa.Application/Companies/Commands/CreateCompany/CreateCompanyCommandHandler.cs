using Budgexa.Application.Companies.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Budgexa.Application.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var currentUserId = currentUserService.UserId;

        var company = Company.Create(
            dto.Name,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            currentUserId,
            dto.Phone, 
            dto.Email,
            null);

        await db.Companies.AddAsync(company, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var created = await db.Companies
            .AsNoTracking()
            .Where(c => c.Id == company.Id)
            .Select(c => new CompanyDto(
                c.Id,
                c.Name,
                c.Description,
                c.Phone,
                c.Email,
                c.StartDate,
                c.EndDate,
                c.CreatedAt,
                c.CreatedByUserId,
                c.UpdatedAt,
                c.UpdatedByUserId))
            .FirstOrDefaultAsync(cancellationToken);

        return created
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve created company.");
    }
}