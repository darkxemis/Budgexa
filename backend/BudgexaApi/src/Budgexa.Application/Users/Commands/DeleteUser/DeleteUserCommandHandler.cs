namespace Budgexa.Application.Users.Commands.DeleteUser;

using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using System.Net;

public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IStatusRepository statusRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "User not found.");

        var deletedStatus = await statusRepository.GetByValueAsync((int)BaseStatus.Delete, cancellationToken)
            ?? throw new AppException(HttpStatusCode.InternalServerError, ErrorTags.Status.NotFound, "Deleted status not found.");

        user.MarkAsDeleted(deletedStatus.Id);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}