namespace Budgexa.Application.Invoices.Queries.GetInvoicesForSelector;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GetInvoicesForSelectorQuery(string? SearchQuery) : IRequest<List<SelectorDto>>;
