namespace Budgexa.Application.Users.Commands.CreateUser;

using System.Net;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IStatusRepository statusRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateUserCommand, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var existingUser = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser is not null)
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.User.EmailAlreadyExists, "Email already exists.");

        var passwordHash = passwordHasher.Hash(dto.Password);

        var newStatus = await statusRepository.GetByValueAsync((int)StatusValue.New, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Status.NotFound, "Status 'New' not found.");

        var user = User.Create(
            dto.Email,
            passwordHash,
            dto.FirstName,
            dto.LastName,
            dto.CompanyId,
            dto.LanguageId,
            newStatus.Id);

        user.SetRoles(dto.RoleIds);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdUser = await userRepository.GetByIdAsync(user.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Server.InternalError, "Failed to retrieve created user.");

        return new UserProfileResult(
            createdUser.Id,
            createdUser.Email,
            createdUser.FirstName,
            createdUser.LastName,
            createdUser.CreatedAt,
            createdUser.Language.Code);
    }
}