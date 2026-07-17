using Budgexa.Application.Companies.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Exceptions;
using MediatR;
using System.Net;

namespace Budgexa.Application.Companies.Queries.GetCompany;

public class GetCompanyQueryHandler : IRequestHandler<GetCompanyQuery, CompanyDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCompanyQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompanyDto> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies.FindAsync(request.Id, cancellationToken);
        
        if (company == null)
        {
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Company.NotFound, $"Company with ID {request.Id} not found.");
        }
        
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