namespace Budgexa.Application.Companies.Commands.CreateCompany;

using Budgexa.Application.Companies.DTOs;
using MediatR;

public sealed record CreateCompanyCommand(CompanyCreateDto Dto) : IRequest<CompanyDto>;