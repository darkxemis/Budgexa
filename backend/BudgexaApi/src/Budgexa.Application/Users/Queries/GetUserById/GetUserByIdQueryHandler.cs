namespace Budgexa.Application.Users.Queries.GetUserById;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetUserByIdQueryHandler(
    IApplicationDbContext db
) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return user
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");
    }
}