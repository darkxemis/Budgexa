namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Languages.DTOs;
using Budgexa.Application.Languages.Queries.GetAllLanguages;
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

        return endpoints;
    }
}