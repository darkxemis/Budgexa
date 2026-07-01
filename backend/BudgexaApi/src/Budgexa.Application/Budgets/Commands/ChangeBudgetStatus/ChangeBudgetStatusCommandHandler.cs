namespace Budgexa.Application.Budgets.Commands.ChangeBudgetStatus;

using System.Net;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Queries;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class ChangeBudgetStatusCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<ChangeBudgetStatusCommand, BudgetDto>
{
    public async Task<BudgetDto> Handle(ChangeBudgetStatusCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var budget = await db.Budgets
            .FirstOrDefaultAsync(b =>
                b.Id == request.Id
                && b.CompanyId == companyId
                && b.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Budget.NotFound, "Budget not found.");

        var statusExists = await db.Statuses
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.Dto.StatusId, cancellationToken);

        if (!statusExists)
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Status.NotFound, "Status not found.");

        budget.ChangeStatus(request.Dto.StatusId, currentUserId);
        await db.SaveChangesAsync(cancellationToken);

        var languageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await BudgetProjections.ProjectByIdAsync(db, budget.Id, languageId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve budget after status change.");
    }
}
