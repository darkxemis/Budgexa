namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Invoices.Commands.ChangeInvoiceStatus;
using Budgexa.Application.Invoices.Commands.CreateInvoice;
using Budgexa.Application.Invoices.Commands.DeleteInvoice;
using Budgexa.Application.Invoices.Commands.DownloadInvoicePdf;
using Budgexa.Application.Invoices.Commands.RegisterInvoicePayment;
using Budgexa.Application.Invoices.Commands.UpdateInvoice;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Invoices.Queries.GenerateInvoiceWithAi;
using Budgexa.Application.Invoices.Queries.GetAllInvoices;
using Budgexa.Application.Invoices.Queries.GetInvoiceById;
using Budgexa.Application.Invoices.Queries.GetInvoicesForSelector;
using Budgexa.Application.Invoices.Queries.GetInvoicesGrid;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class InvoicesEndpoints
{
    public static IEndpointRouteBuilder MapInvoicesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/invoices").WithTags("Invoices");

        group.MapGet("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetInvoiceById")
            .WithSummary("GET /api/v1/invoices/{id}")
            .WithDescription("Returns a specific invoice of the authenticated user's company by id, including lines.");

        group.MapGet("/",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAllInvoicesQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<IEnumerable<InvoiceGridDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetAllInvoices")
            .WithSummary("GET /api/v1/invoices")
            .WithDescription("Returns all non-deleted invoices from the authenticated user's company.");

        group.MapPost("/grid",
            async (GridRequestDto request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetInvoicesGridQuery(request), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<GridResponseDto<InvoiceGridDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetInvoicesGrid")
            .WithSummary("POST /api/v1/invoices/grid")
            .WithDescription("Returns a paginated, filtered, and sorted grid of invoices with support for advanced filtering, ordering, and global search.");

        group.MapGet("/selector",
            async (string? searchQuery, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetInvoicesForSelectorQuery(searchQuery), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<SelectorDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetInvoicesForSelector")
            .WithSummary("GET /api/v1/invoices/selector")
            .WithDescription("Returns invoices for selector usage with optional search filter.");

        group.MapPost("/",
            async (InvoiceCreateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateInvoiceCommand(dto), cancellationToken);
                return Results.Created($"/api/v1/invoices/{result.Id}", result);
            })
            .RequireAuthorization()
            .Produces<InvoiceDto>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("CreateInvoice")
            .WithSummary("POST /api/v1/invoices")
            .WithDescription("Creates a new invoice with optional lines.");

        group.MapPut("/{id:guid}",
            async (Guid id, InvoiceUpdateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new UpdateInvoiceCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("UpdateInvoice")
            .WithSummary("PUT /api/v1/invoices/{id}")
            .WithDescription("Updates an existing invoice header and replaces its lines (diff by id).");

        group.MapPatch("/{id:guid}/status",
            async (Guid id, ChangeInvoiceStatusDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new ChangeInvoiceStatusCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("ChangeInvoiceStatus")
            .WithSummary("PATCH /api/v1/invoices/{id}/status")
            .WithDescription("Changes the status of an existing invoice.");

        group.MapPost("/{id:guid}/payments",
            async (Guid id, RegisterInvoicePaymentDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new RegisterInvoicePaymentCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("RegisterInvoicePayment")
            .WithSummary("POST /api/v1/invoices/{id}/payments")
            .WithDescription("Registers a payment against an invoice. Automatically updates the invoice status to PartiallyPaid or Paid based on the total amount paid.");

        group.MapGet("/{id:guid}/pdf",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new DownloadInvoicePdfCommand(id), cancellationToken);
                return Results.File(result.PdfBytes, "application/pdf", result.FileName);
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DownloadInvoicePdf")
            .WithSummary("GET /api/v1/invoices/{id}/pdf")
            .WithDescription("Returns the invoice PDF for the authenticated user's company.");

        group.MapDelete("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteInvoiceCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DeleteInvoice")
            .WithSummary("DELETE /api/v1/invoices/{id}")
            .WithDescription("Soft deletes an invoice by setting its status to deleted.");

        group.MapPost("/generate-with-ai",
            async ([FromBody] PrivateInvoiceAiRequestDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GenerateInvoiceWithAiQuery(dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<PrivateInvoiceAiResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError)
            .WithName("GenerateInvoiceWithAi")
            .WithSummary("POST /api/v1/invoices/generate-with-ai")
            .WithDescription("Generates an invoice using AI from natural language text or voice input. Supports multi-language input, returns resolved customer ID, series, parsed dates, and matched items.");

        return endpoints;
    }
}
