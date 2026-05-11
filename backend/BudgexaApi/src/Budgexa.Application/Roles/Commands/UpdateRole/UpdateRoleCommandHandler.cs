namespace Budgexa.Application.Roles.Commands.UpdateRole;

using System.Net;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.Role.NotFound, "Role not found.");

        var dto = request.Dto;

        var existingRole = await roleRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingRole is not null && existingRole.Id != request.Id)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Role.NameExists, "Role name already exists.");

        role.Update(dto.Name);

        roleRepository.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}