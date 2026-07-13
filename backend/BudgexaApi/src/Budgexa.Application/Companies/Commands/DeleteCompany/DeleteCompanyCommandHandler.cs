using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Exceptions;
using MediatR;
using System.Net;

namespace Budgexa.Application.Companies.Commands.DeleteCompany;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCompanyCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies.FindAsync(request.Id, cancellationToken);
        
        if (company == null)
        {
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Company.NotFound, $"Company with ID {request.Id} not found.");
        }
        
        _dbContext.Companies.Remove(company);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}