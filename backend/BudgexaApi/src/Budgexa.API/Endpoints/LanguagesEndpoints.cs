namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Languages.DTOs;
using Budgexa.Application.Languages.Queries.GetAllLanguages;
using Budgexa.Application.Languages.Queries.GetLanguagesForSelector;
using MediatR;

public static class LanguagesEndpoints
{
    public static IEndpointRouteBuilder MapLanguagesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/languages").WithTags("Languages");

        group.MapGet("",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAllLanguagesQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<LanguageDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("GetAllLanguages")
            .WithSummary("GET /api/v1/languages")
            .WithDescription("Returns all available languages for selection.");

        group.MapGet("/selector",
            async (
                ISender sender,
                string? searchQuery,
                CancellationToken cancellationToken) =>
            {
                var query = new GetLanguagesForSelectorQuery(searchQuery);
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<SelectorDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("GetLanguagesForSelector")
            .WithSummary("GET /api/v1/languages/selector")
            .WithDescription("Returns languages for selector with translations. Optional filter: searchQuery.");

        return endpoints;
    }
}