using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

namespace Budgexa.Application.Roles.Commands.DeleteRole;

public sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository
) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
            throw new AppException(System.Net.HttpStatusCode.NotFound, ErrorTags.Role.NotFound, "Role not found.");

        await roleRepository.DeleteAsync(role, cancellationToken);
    }
}