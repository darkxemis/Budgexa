using Budgexa.Application.Companies.DTOs;
using Budgexa.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Budgexa.Application.Companies.Queries.GetCompanies;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, List<CompanyDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCompaniesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _dbContext.Companies
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
            .ToListAsync(cancellationToken);
            
        return companies;
    }
}