namespace Budgexa.Application.Auth.Commands.Register;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
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

        var statusId = await db.Statuses
            .AsNoTracking()
            .Where(s => s.Value == (int)BaseStatus.New)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (statusId is null)
            throw new AppException(HttpStatusCode.BadRequest, ErrorTags.Status.NotFound, "Default status 'New' not found.");

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, hash, request.FirstName, request.LastName, request.CompanyId, request.LanguageId, statusId.Value);

        foreach (var roleId in request.RoleIds)
        {
            user.UserRoles.Add(UserRole.Create(user.Id, roleId));
        }

        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
