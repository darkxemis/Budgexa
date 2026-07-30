namespace Budgexa.Application.Users.Commands.DeleteSignatureImage;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteSignatureImageCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService
) : IRequestHandler<DeleteSignatureImageCommand, UserProfileResult>
{
    public async Task<UserProfileResult> Handle(DeleteSignatureImageCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "The requested user was not found.");

        if (!string.IsNullOrWhiteSpace(user.SignatureUrl))
            await fileStorageService.DeleteSignatureImageAsync(user.SignatureUrl, cancellationToken);

        user.SetSignature(null);
        await db.SaveChangesAsync(cancellationToken);

        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileResult(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.CompanyId,
                u.Company.Name,
                u.LanguageId,
                u.Language.Code,
                u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                u.CreatedAt,
                u.UpdatedAt,
                u.ProfileImageUrl,
                u.SignatureUrl))
            .FirstAsync(cancellationToken);
    }
}
