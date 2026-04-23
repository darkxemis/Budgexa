namespace Budgexa.Application.Auth.Commands.Register;

using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using System.Net;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existingUser is not null)
        {
            throw new AppException(HttpStatusCode.Conflict, ErrorTags.Auth.EmailAlreadyExists, "A user with this email already exists.");
        }

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, hash, request.FirstName, request.LastName, request.CompanyId, request.LanguageId, request.StatusId);

        foreach (var roleId in request.RoleIds)
        {
            user.UserRoles.Add(UserRole.Create(user.Id, roleId));
        }

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
