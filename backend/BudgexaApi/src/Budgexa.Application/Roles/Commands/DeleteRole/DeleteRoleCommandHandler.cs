using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Budgexa.Application.Roles.Commands.DeleteRole;

public sealed class DeleteRoleCommandHandler(
    IApplicationDbContext db
) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await db.Roles
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role is null)
            throw new AppException(System.Net.HttpStatusCode.NotFound, ErrorTags.Role.NotFound, "Role not found.");

        db.Roles.Remove(role);
        await db.SaveChangesAsync(cancellationToken);
    }
}