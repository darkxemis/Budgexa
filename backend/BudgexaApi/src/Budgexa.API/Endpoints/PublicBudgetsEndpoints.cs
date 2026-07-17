namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.PublicBudgets.Commands.ConfirmAndDownload;
using Budgexa.Application.PublicBudgets.DTOs;
using Budgexa.Application.PublicBudgets.Queries.GeneratePublicBudgetWithAi;
using Budgexa.Application.PublicBudgets.Queries.GetPublicItems;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class PublicBudgetsEndpoints
{
    public static IEndpointRouteBuilder MapPublicBudgetsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/public-budgets")
            .WithTags("Public Budgets")
            .RequireRateLimiting("PublicBudgetLimit");

        group.MapPost("ai-generate",
            async (
                ISender sender,
                [FromBody] PublicBudgetAiRequestDto request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GeneratePublicBudgetWithAiQuery(request), cancellationToken);
                return Results.Ok(result);
            })
            .AllowAnonymous()
            .Produces<PublicBudgetAiResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests)
            .WithName("GeneratePublicBudgetWithAi")
            .WithSummary("POST /api/v1/public-budgets/ai-generate")
            .WithDescription("Generates budget items from free-text using AI, matched against the company's item catalog. No data is saved.");

        group.MapPost("confirm-and-download",
            async (
                ISender sender,
                [FromBody] ConfirmPublicBudgetRequestDto request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new ConfirmAndDownloadCommand(request), cancellationToken);

                return Results.File(
                    result.PdfBytes,
                    contentType: "application/pdf",
                    fileDownloadName: result.FileName);
            })
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests)
            .WithName("ConfirmAndDownloadPublicBudget")
            .WithSummary("POST /api/v1/public-budgets/confirm-and-download")
            .WithDescription("Validates items server-side, calculates totals from authoritative prices, saves the budget, generates a PDF, and returns it for download.");

        group.MapGet("{companyId:guid}/items",
            async (
                Guid companyId,
                string? searchQuery,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetPublicItemsQuery(companyId, searchQuery), cancellationToken);
                return Results.Ok(result);
            })
            .AllowAnonymous()
            .RequireRateLimiting("PublicItemSearchLimit")
            .Produces<List<PublicItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status429TooManyRequests)
            .WithName("GetPublicItems")
            .WithSummary("GET /api/v1/public-budgets/{companyId}/items")
            .WithDescription("Returns available items from the company's catalog for public budget creation. Supports optional search filtering.");

        return endpoints;
    }
}
