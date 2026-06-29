namespace Budgexa.Application.Roles.Queries.GetAllRoles;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Roles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllRolesQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var isSuperAdmin = currentUserService.Roles.Contains("superadministrator");

        var query = db.Roles.AsNoTracking();

        if (!isSuperAdmin)
        {
            query = query.Where(r => r.Name == "administrator" || r.Name == "freelance");
        }

        return await query
            .Select(r => new RoleDto(r.Id, r.Name))
            .ToListAsync(cancellationToken);
    }
}