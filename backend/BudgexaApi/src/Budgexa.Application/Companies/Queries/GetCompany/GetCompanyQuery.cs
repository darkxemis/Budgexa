namespace Budgexa.Application.Companies.Queries.GetCompany;

using Budgexa.Application.Companies.DTOs;
using MediatR;

public sealed record GetCompanyQuery(Guid Id) : IRequest<CompanyDto>;