namespace Budgexa.Application.Users.Queries.GetCurrentUser;

using MediatR;
using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;

public sealed class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetCurrentUserQuery, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        return new UserProfileResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CompanyId,
            user.Company.Name,
            user.LanguageId,
            user.Language.Code,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
