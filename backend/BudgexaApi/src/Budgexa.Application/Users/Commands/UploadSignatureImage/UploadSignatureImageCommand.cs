namespace Budgexa.Application.Users.Commands.UploadSignatureImage;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record UploadSignatureImageCommand(
    Stream FileStream,
    string FileName) : IRequest<UserProfileResult>;
