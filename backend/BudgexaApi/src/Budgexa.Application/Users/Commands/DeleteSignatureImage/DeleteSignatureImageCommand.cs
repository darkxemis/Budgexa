namespace Budgexa.Application.Users.Commands.DeleteSignatureImage;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record DeleteSignatureImageCommand : IRequest<UserProfileResult>;
