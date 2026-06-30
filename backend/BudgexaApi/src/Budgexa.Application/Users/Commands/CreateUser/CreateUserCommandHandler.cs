namespace Budgexa.Application.Users.Commands.CreateUser;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class CreateUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var emailExists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == dto.Email, cancellationToken);
        if (emailExists)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.User.EmailAlreadyExists, "Email already exists.");

        var passwordHash = passwordHasher.Hash(dto.Password);

        var newStatus = await db.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Value == 1, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Status.NotFound, "Status 'New' not found.");

        var companyId = currentUserService.CompanyId;

        var user = User.Create(
            dto.Email,
            passwordHash,
            dto.FirstName,
            dto.LastName,
            companyId,
            dto.LanguageId,
            newStatus.Id);

        user.SetRoles(dto.RoleIds);

        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var createdUser = await db.Users
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

        return createdUser
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve created user.");
    }
}