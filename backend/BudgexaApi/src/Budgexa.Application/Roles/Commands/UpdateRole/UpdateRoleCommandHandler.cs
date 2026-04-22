using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using System.Net;
using MediatR;

namespace Budgexa.Application.Roles.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
            throw new AppException(HttpStatusCode.NotFound, ErrorTags.Role.NotFound, "Role not found.");

        var exists = await roleRepository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists && !string.Equals(role.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Role.NameExists, "Role name already exists.");

        role.UpdateName(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}