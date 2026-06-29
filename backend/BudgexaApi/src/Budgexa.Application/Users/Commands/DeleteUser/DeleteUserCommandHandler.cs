namespace Budgexa.Application.Users.Commands.DeleteUser;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

public sealed class DeleteUserCommandHandler(
    IApplicationDbContext db
) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "User not found.");

        var deletedStatusId = await db.Statuses
            .AsNoTracking()
            .Where(s => s.Value == (int)BaseStatus.Delete)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Status.NotFound, "Deleted status not found.");

        user.MarkAsDeleted(deletedStatusId);
        await db.SaveChangesAsync(cancellationToken);
    }
}