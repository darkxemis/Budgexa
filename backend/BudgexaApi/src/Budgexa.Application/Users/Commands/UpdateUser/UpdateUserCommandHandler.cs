namespace Budgexa.Application.Users.Commands.UpdateUser;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher
) : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        var dto = request.Dto;

        if (user.Email != dto.Email)
        {
            var emailTaken = await db.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == dto.Email, cancellationToken);
            if (emailTaken)
                throw new AppException(HttpStatusCode.Conflict, ErrorTags.User.EmailAlreadyExists, "Email already exists.");
        }

        var passwordHash = string.IsNullOrWhiteSpace(dto.Password)
            ? user.PasswordHash
            : passwordHasher.Hash(dto.Password);

        user.Update(
            dto.Email,
            passwordHash,
            dto.FirstName,
            dto.LastName,
            user.CompanyId,
            dto.LanguageId);

        user.SetRoles(dto.RoleIds);

        await db.SaveChangesAsync(cancellationToken);

        var updatedUser = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == user.Id)
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

        return updatedUser
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated user.");
    }
}