namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Budgets.Commands.ChangeBudgetStatus;
using Budgexa.Application.Budgets.Commands.CreateBudget;
using Budgexa.Application.Budgets.Commands.DeleteBudget;
using Budgexa.Application.Budgets.Commands.UpdateBudget;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Queries.GenerateBudgetWithAi;
using Budgexa.Application.Budgets.Queries.GetAllBudgets;
using Budgexa.Application.Budgets.Queries.GetBudgetById;
using Budgexa.Application.Budgets.Queries.GetBudgetsForSelector;
using Budgexa.Application.Budgets.Queries.GetBudgetsGrid;
using Budgexa.Application.Common.DTOs;
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

        group.MapGet("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetBudgetByIdQuery(id), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<BudgetDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetBudgetById")
            .WithSummary("GET /api/v1/budgets/{id}")
            .WithDescription("Returns a specific budget of the authenticated user's company by id, including lines.");

        group.MapGet("/",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAllBudgetsQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<IEnumerable<BudgetGridDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetAllBudgets")
            .WithSummary("GET /api/v1/budgets")
            .WithDescription("Returns all non-deleted budgets from the authenticated user's company.");

        group.MapPost("/grid",
            async (GridRequestDto request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetBudgetsGridQuery(request), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<GridResponseDto<BudgetGridDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetBudgetsGrid")
            .WithSummary("POST /api/v1/budgets/grid")
            .WithDescription("Returns a paginated, filtered, and sorted grid of budgets with support for advanced filtering, ordering, and global search.");

        group.MapGet("/selector",
            async (string? searchQuery, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetBudgetsForSelectorQuery(searchQuery), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<SelectorDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetBudgetsForSelector")
            .WithSummary("GET /api/v1/budgets/selector")
            .WithDescription("Returns budgets for selector usage with optional search filter.");

        group.MapPost("/",
            async (BudgetCreateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateBudgetCommand(dto), cancellationToken);
                return Results.Created($"/api/v1/budgets/{result.Id}", result);
            })
            .RequireAuthorization()
            .Produces<BudgetDto>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("CreateBudget")
            .WithSummary("POST /api/v1/budgets")
            .WithDescription("Creates a new budget with optional lines.");

        group.MapPut("/{id:guid}",
            async (Guid id, BudgetUpdateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new UpdateBudgetCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<BudgetDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("UpdateBudget")
            .WithSummary("PUT /api/v1/budgets/{id}")
            .WithDescription("Updates an existing budget header and replaces its lines (diff by id).");

        group.MapPatch("/{id:guid}/status",
            async (Guid id, ChangeBudgetStatusDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new ChangeBudgetStatusCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<BudgetDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("ChangeBudgetStatus")
            .WithSummary("PATCH /api/v1/budgets/{id}/status")
            .WithDescription("Changes the status of an existing budget.");

        group.MapDelete("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteBudgetCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DeleteBudget")
            .WithSummary("DELETE /api/v1/budgets/{id}")
            .WithDescription("Soft deletes a budget by setting its status to deleted.");

        return endpoints;
    }
}