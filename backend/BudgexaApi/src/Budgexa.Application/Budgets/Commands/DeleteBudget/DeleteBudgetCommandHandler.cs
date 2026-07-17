namespace Budgexa.Application.Budgets.Commands.DeleteBudget;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteBudgetCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<DeleteBudgetCommand>
{
    public async Task Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
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

        budget.ChangeStatus(StatusIds.Delete, currentUserId);
        await db.SaveChangesAsync(cancellationToken);
    }
}
