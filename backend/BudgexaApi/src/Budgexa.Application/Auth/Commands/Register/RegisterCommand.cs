namespace Budgexa.Application.Auth.Commands.Register;

using MediatR;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid CompanyId,
    Guid LanguageId,
    Guid StatusId,
    Guid[] RoleIds) : IRequest<Guid>;
