namespace Budgexa.Application.Items.Queries.GetItemsForSelector;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GetItemsForSelectorQuery(string? SearchQuery) : IRequest<List<SelectorDto>>;
