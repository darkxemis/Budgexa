namespace Budgexa.Application.Users.Queries.GetAllUsers;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetAllUsersQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllUsersQuery, IEnumerable<UserProfileResult>>
{
    public async Task<IEnumerable<UserProfileResult>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllByCompanyIdAsync(currentUserService.CompanyId, cancellationToken);

        return users.Select(user => new UserProfileResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.Language.Code));
    }
}