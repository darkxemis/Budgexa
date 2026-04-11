namespace Budgexa.Application.Users.Queries.GetCurrentUser;

using MediatR;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Interfaces;

public sealed class GetCurrentUserQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetCurrentUserQuery, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        return new UserProfileResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt);
    }
}
