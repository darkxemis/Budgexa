namespace Budgexa.Application.Roles.Commands.CreateRole;

using System.Net;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateRoleCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var existingRole = await roleRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingRole is not null)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Role.NameExists, "Role name already exists.");

        var role = Role.Create(dto.Name);

        await roleRepository.AddAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}