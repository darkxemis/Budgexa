namespace Budgexa.Application.Companies.Commands.DeleteCompany;

using MediatR;

public sealed record DeleteCompanyCommand(Guid Id) : IRequest;