namespace Budgexa.Application.Users.Commands.UpdateCurrentUser;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class UpdateCurrentUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCurrentUserCommand, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var user = await userRepository.GetByIdForUpdateAsync(userId, cancellationToken)
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
      
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedUser = await userRepository.GetByIdAsync(user.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve updated user.");

        return new UserProfileResult(
            updatedUser.Id,
            updatedUser.Email,
            updatedUser.FirstName,
            updatedUser.LastName,
            updatedUser.CompanyId,
            updatedUser.Company.Name,
            updatedUser.LanguageId,
            updatedUser.Language.Code,
            updatedUser.CreatedAt,
            updatedUser.UpdatedAt);
    }
}