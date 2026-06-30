namespace Budgexa.Application.Roles.Commands.CreateRole;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class CreateRoleCommandHandler(
    IApplicationDbContext db
) : IRequestHandler<CreateRoleCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var nameExists = await db.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Name == dto.Name, cancellationToken);
        if (nameExists)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Role.NameExists, "Role name already exists.");

        var role = Role.Create(dto.Name);

        await db.Roles.AddAsync(role, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}