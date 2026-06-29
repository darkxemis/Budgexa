namespace Budgexa.Application.Users.Commands.UpdateCurrentUser;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateCurrentUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService
) : IRequestHandler<UpdateCurrentUserCommand, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        var dto = request.Dto;

        var passwordHash = string.IsNullOrWhiteSpace(dto.Password)
            ? user.PasswordHash
            : passwordHasher.Hash(dto.Password);

        user.Update(
            user.Email,
            passwordHash,
            dto.FirstName,
            dto.LastName,
            user.CompanyId,
            dto.LanguageId);

        await db.SaveChangesAsync(cancellationToken);

        var updatedUser = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == user.Id)
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

        return updatedUser
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated user.");
    }
}