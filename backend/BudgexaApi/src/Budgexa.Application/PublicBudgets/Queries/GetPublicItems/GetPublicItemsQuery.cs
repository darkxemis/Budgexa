namespace Budgexa.Application.PublicBudgets.Queries.GetPublicItems;

using Budgexa.Application.PublicBudgets.DTOs;
using MediatR;

public sealed record GetPublicItemsQuery(
    Guid CompanyId,
    string? SearchQuery) : IRequest<List<PublicItemDto>>;
