namespace Budgexa.Application.Budgets.Queries.GetBudgetsForSelector;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GetBudgetsForSelectorQuery(string? SearchQuery) : IRequest<List<SelectorDto>>;
