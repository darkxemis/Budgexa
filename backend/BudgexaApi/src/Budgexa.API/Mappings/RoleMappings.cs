namespace Budgexa.API.Mappings;

using Budgexa.Application.Roles.Commands.UpdateRole;
using Budgexa.Application.Roles.DTOs;

public static class RoleMappings
{
    public static UpdateRoleCommand ToCommand(this UpdateRoleDto request, Guid id)
        => new(id, request.Name);
}

