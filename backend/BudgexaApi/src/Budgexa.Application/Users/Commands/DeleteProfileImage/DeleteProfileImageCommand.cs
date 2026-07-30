namespace Budgexa.Application.Users.Commands.DeleteProfileImage;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record DeleteProfileImageCommand : IRequest<UserProfileResult>;
