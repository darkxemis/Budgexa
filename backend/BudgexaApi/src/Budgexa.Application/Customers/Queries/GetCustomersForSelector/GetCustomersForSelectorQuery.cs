namespace Budgexa.Application.Customers.Queries.GetCustomersForSelector;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GetCustomersForSelectorQuery(string? SearchQuery) : IRequest<List<SelectorDto>>;
