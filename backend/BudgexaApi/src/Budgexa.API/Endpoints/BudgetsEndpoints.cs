namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Queries.GenerateBudgetWithAi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class BudgetsEndpoints
{
    public static IEndpointRouteBuilder MapBudgetsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/budgets").WithTags("Budgets");

        group.MapPost("ai-generate",
            async (
                ISender sender,
                [FromBody] GenerateBudgetWithAiRequestDto request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GenerateBudgetWithAiQuery(request), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<BudgetItemsAiResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GenerateBudgetWithAi")
            .WithSummary("POST /api/v1/budgets/ai-generate")
            .WithDescription("Generates budget items from free-text using AI (Ollama) to extract products and quantities.");

        return endpoints;
    }
}