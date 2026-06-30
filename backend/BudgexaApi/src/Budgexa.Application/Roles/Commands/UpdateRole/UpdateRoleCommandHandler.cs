namespace Budgexa.Application.Roles.Commands.UpdateRole;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateRoleCommandHandler(
    IApplicationDbContext db
) : IRequestHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await db.Roles
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Role.NotFound, "Role not found.");

        var dto = request.Dto;

        var nameTaken = await db.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Name == dto.Name && r.Id != request.Id, cancellationToken);
        if (nameTaken)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Role.NameExists, "Role name already exists.");

        role.Update(dto.Name);

        await db.SaveChangesAsync(cancellationToken);
    }
}