using Budgexa.Application.Companies.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Exceptions;
using MediatR;
using System.Net;

namespace Budgexa.Application.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCompanyCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies.FindAsync(request.Id, cancellationToken);
        
        if (company == null)
        {
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Company.NotFound, $"Company with ID {request.Id} not found.");
        }
        
        var currentUserId = _currentUserService.UserId;
        
        company.Update(
            request.Dto.Name,
            request.Dto.Description,
            request.Dto.Phone,
            request.Dto.Email,
            request.Dto.EndDate,
            currentUserId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CompanyDto(
            company.Id,
            company.Name,
            company.Description,
            company.Phone,
            company.Email,
            company.StartDate,
            company.EndDate,
            company.CreatedAt,
            company.CreatedByUserId,
            company.UpdatedAt,
            company.UpdatedByUserId);
    }
}