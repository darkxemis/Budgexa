namespace Budgexa.Application.Auth.Commands.Register;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

public sealed class RegisterCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher
) : IRequestHandler<RegisterCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.User.EmailAlreadyExists, "A user with this email already exists.");

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, hash, request.FirstName, request.LastName, request.CompanyId, request.LanguageId, StatusIds.New);

        foreach (var roleId in request.RoleIds)
        {
            user.UserRoles.Add(UserRole.Create(user.Id, roleId));
        }

        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
