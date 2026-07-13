namespace Budgexa.Application.Companies.Commands.UpdateCompany;

using Budgexa.Application.Companies.DTOs;
using MediatR;

public sealed record UpdateCompanyCommand(Guid Id, CompanyUpdateDto Dto) : IRequest<CompanyDto>;