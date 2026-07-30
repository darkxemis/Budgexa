namespace Budgexa.Application.Users.Commands.UploadProfileImage;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record UploadProfileImageCommand(
    Stream FileStream,
    string FileName) : IRequest<UserProfileResult>;
