namespace Budgexa.Application.Users.Queries.GetAllUsers;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllUsersQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var companyId = currentUserService.CompanyId;

        return await db.Users
            .AsNoTracking()
            .Where(u => u.CompanyId == companyId)
            .Where(u => u.Status.Value != (int)BaseStatus.Delete)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                new CompanyInfo(u.Company.Id, u.Company.Name),
                new LanguageInfo(u.Language.Id, u.Language.Name),
                u.UserRoles.Select(ur => new RoleInfo(ur.Role.Id, ur.Role.Name)).ToList(),
                u.CreatedAt,
                u.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}