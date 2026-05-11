namespace Budgexa.Application.Users.Queries.GetUserById;

using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using System.Net;

public sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository
) : IRequestHandler<GetUserByIdQuery, UserProfileResult?>
{
    public async Task<UserProfileResult?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        return new UserProfileResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.Language.Code);
    }
}