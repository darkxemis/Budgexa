namespace Budgexa.Application.Languages.Queries.GetLanguagesForSelector;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GetLanguagesForSelectorQuery(
    string? SearchQuery = null
) : IRequest<List<SelectorDto>>;
