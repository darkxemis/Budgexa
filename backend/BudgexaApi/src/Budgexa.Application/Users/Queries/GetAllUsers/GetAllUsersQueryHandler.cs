namespace Budgexa.Application.Users.Queries.GetAllUsers;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetAllUsersQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllByCompanyIdAsync(currentUserService.CompanyId, cancellationToken);

        return users.Select(user => new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            new CompanyInfo(user.Company.Id, user.Company.Name),
            new LanguageInfo(user.Language.Id, user.Language.Name),
            user.UserRoles.Select(ur => new RoleInfo(ur.Role.Id, ur.Role.Name)).ToList(),
            user.CreatedAt,
            user.UpdatedAt));
    }
}