namespace Budgexa.Application.Budgets.Queries.GetBudgetById;

using System.Net;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetBudgetByIdQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetBudgetByIdQuery, BudgetDto>
{
    public async Task<BudgetDto> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var exists = await db.Budgets
            .AsNoTracking()
            .AnyAsync(b =>
                b.Id == request.Id
                && b.CompanyId == companyId
                && b.StatusId != StatusIds.Delete,
                cancellationToken);

        if (!exists)
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Budget.NotFound, "Budget not found.");

        return await BudgetProjections.ProjectByIdAsync(db, request.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Budget.NotFound, "Budget not found.");
    }
}
