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
) : IRequestHandler<UpdateUserCommand, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        var dto = request.Dto;

        if (user.Email != dto.Email)
        {
            var existingUser = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingUser is not null)
                throw new AppException(HttpStatusCode.Conflict, ErrorTags.User.EmailAlreadyExists, "Email already exists.");
        }

        var passwordHash = passwordHasher.Hash(dto.Password);

        user.Update(
            dto.Email,
            passwordHash,
            dto.FirstName,
            dto.LastName,
            dto.CompanyId,
            dto.LanguageId);

        user.SetRoles(dto.RoleIds);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedUser = await userRepository.GetByIdAsync(user.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated user.");

        return new UserProfileResult(
            updatedUser.Id,
            updatedUser.Email,
            updatedUser.FirstName,
            updatedUser.LastName,
            updatedUser.CreatedAt,
            updatedUser.Language.Code);
    }
}