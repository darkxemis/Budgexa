namespace Budgexa.Application.Status.Queries.GetStatusForSelector;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GetStatusForSelectorQuery(
    string? Group = null,
    string? SearchQuery = null
) : IRequest<List<SelectorDto>>;
