namespace Budgexa.Application.Auth.Commands.RefreshToken;

using Budgexa.Application.Auth.DTOs;
using MediatR;

public sealed record RefreshTokenCommand(string Token) : IRequest<AuthResult>;
