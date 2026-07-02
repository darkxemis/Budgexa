namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.PublicBudgets.Commands.ConfirmAndDownload;
using Budgexa.Application.PublicBudgets.DTOs;
using Budgexa.Application.PublicBudgets.Queries.GeneratePublicBudgetWithAi;
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

        return endpoints;
    }
}
