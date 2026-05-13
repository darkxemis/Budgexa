namespace Budgexa.Application.Users.Commands.UpdateUser;

using System.Net;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        var dto = request.Dto;

        if (user.Email != dto.Email)
        {
            var existingUser = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingUser is not null)
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedUser = await userRepository.GetByIdAsync(user.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated user.");

        return new UserDto(
            updatedUser.Id,
            updatedUser.Email,
            updatedUser.FirstName,
            updatedUser.LastName,
            new CompanyInfo(updatedUser.Company.Id, updatedUser.Company.Name),
            new LanguageInfo(updatedUser.Language.Id, updatedUser.Language.Name),
            updatedUser.UserRoles.Select(ur => new RoleInfo(ur.Role.Id, ur.Role.Name)).ToList(),
            updatedUser.CreatedAt,
            updatedUser.UpdatedAt);
    }
}