namespace Budgexa.Application.Roles.Queries.GetAllRoles;

using Budgexa.Application.Roles.DTOs;
using Budgexa.Application.Roles.Mappings;
using Budgexa.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

public sealed class GetAllRolesQueryHandler(
    IRoleRepository roleRepository,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var isSuperAdmin = user?.IsInRole("superadministrator") ?? false;

        var roles = await roleRepository.GetAllAsync(cancellationToken);

        if (!isSuperAdmin)
        {
            roles = roles.Where(r => r.Name == "administrator" || r.Name == "freelance").ToList();
        }

        return roles.Select(RoleMappings.ToDto).ToList();
    }
}