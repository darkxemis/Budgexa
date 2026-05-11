namespace Budgexa.Application.Roles.Queries.GetAllRoles;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Roles.DTOs;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetAllRolesQueryHandler(
    IRoleRepository roleRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken);

        var userRoles = currentUserService.Roles;
        var isSuperAdmin = userRoles.Contains("superadministrator");

        if (!isSuperAdmin)
        {
            roles = roles.Where(r => r.Name == "administrator" || r.Name == "freelance").ToList();
        }

        return roles.Select(role => new RoleDto(role.Id, role.Name)).ToList();
    }
}