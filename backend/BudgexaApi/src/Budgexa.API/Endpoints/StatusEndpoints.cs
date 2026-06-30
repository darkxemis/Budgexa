namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Status.Queries.GetStatusForSelector;
using MediatR;

public static class StatusEndpoints
{
    public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/status").WithTags("Status");

        group.MapGet("/selector",
            async (
                ISender sender,
                string? group,
                string? searchQuery,
                CancellationToken cancellationToken) =>
            {
                var query = new GetStatusForSelectorQuery(group, searchQuery);
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<SelectorDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("GetStatusForSelector")
            .WithSummary("GET /api/v1/status/selector")
            .WithDescription("Returns status for selector with translations. Optional filters: group and searchQuery.");

        return endpoints;
    }
}
