namespace Budgexa.Application.Items.Commands.DeleteItem;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteItemCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<DeleteItemCommand>
{
    public async Task Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;
        var currentUserId = currentUserService.UserId;

        var item = await db.Items
            .FirstOrDefaultAsync(i =>
                i.Id == request.Id
                && i.CompanyId == companyId
                && i.StatusId != StatusIds.Delete,
                cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Item.NotFound, "Item not found.");

        item.MarkAsDeleted(StatusIds.Delete, currentUserId);
        await db.SaveChangesAsync(cancellationToken);
    }
}
