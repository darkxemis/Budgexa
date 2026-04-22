namespace Budgexa.Application.Roles.Mappings;

using Budgexa.Application.Roles.DTOs;
using Budgexa.Domain.Entities;

public static class RoleMappings
{
    public static RoleDto ToDto(this Role role)
        => new(role.Id, role.Name);
}
