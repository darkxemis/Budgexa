namespace Budgexa.Application.Users.Queries.GetUserById;

using System.Net;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository
) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            new CompanyInfo(user.Company.Id, user.Company.Name),
            new LanguageInfo(user.Language.Id, user.Language.Name),
            user.UserRoles.Select(ur => new RoleInfo(ur.Role.Id, ur.Role.Name)).ToList(),
            user.CreatedAt,
            user.UpdatedAt);
    }
}