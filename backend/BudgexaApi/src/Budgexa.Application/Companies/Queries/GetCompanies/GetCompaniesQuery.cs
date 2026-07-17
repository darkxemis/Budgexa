namespace Budgexa.Application.Companies.Queries.GetCompanies;

using Budgexa.Application.Companies.DTOs;
using MediatR;

public sealed record GetCompaniesQuery : IRequest<List<CompanyDto>>;