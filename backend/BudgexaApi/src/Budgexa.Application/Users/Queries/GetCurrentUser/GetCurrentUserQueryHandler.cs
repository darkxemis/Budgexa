namespace Budgexa.Application.Users.Queries.GetCurrentUser;

using MediatR;
using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

public sealed class GetCurrentUserQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetCurrentUserQuery, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var profile = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileResult(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.CompanyId,
                u.Company.Name,
                u.LanguageId,
                u.Language.Code,
                u.CreatedAt,
                u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return profile
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");
    }
}
