namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Items.Commands.CreateItem;
using Budgexa.Application.Items.Commands.DeleteItem;
using Budgexa.Application.Items.Commands.UpdateItem;
using Budgexa.Application.Items.DTOs;
using Budgexa.Application.Items.Queries.GetAllItems;
using Budgexa.Application.Items.Queries.GetItemById;
using Budgexa.Application.Items.Queries.GetItemsForSelector;
using Budgexa.Application.Items.Queries.GetItemsGrid;
using MediatR;

public static class ItemsEndpoints
{
    public static IEndpointRouteBuilder MapItemsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/items").WithTags("Items");

        group.MapGet("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetItemByIdQuery(id), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetItemById")
            .WithSummary("GET /api/v1/items/{id}")
            .WithDescription("Returns a specific item of the authenticated user's company by id.");

        group.MapGet("/",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAllItemsQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<IEnumerable<ItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetAllItems")
            .WithSummary("GET /api/v1/items")
            .WithDescription("Returns all non-deleted items from the authenticated user's company.");

        group.MapPost("/grid",
            async (GridRequestDto request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetItemsGridQuery(request), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<GridResponseDto<ItemGridDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetItemsGrid")
            .WithSummary("POST /api/v1/items/grid")
            .WithDescription("Returns a paginated, filtered, and sorted grid of items with support for advanced filtering, ordering, and global search.");

        group.MapGet("/selector",
            async (string? searchQuery, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetItemsForSelectorQuery(searchQuery), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<SelectorDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetItemsForSelector")
            .WithSummary("GET /api/v1/items/selector")
            .WithDescription("Returns items for selector usage with optional search filter.");

        group.MapPost("/",
            async (ItemCreateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateItemCommand(dto), cancellationToken);
                return Results.Created($"/api/v1/items/{result.Id}", result);
            })
            .RequireAuthorization()
            .Produces<ItemDto>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("CreateItem")
            .WithSummary("POST /api/v1/items")
            .WithDescription("Creates a new item in the authenticated user's company.");

        group.MapPut("/{id:guid}",
            async (Guid id, ItemUpdateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new UpdateItemCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("UpdateItem")
            .WithSummary("PUT /api/v1/items/{id}")
            .WithDescription("Updates an existing item.");

        group.MapDelete("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteItemCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DeleteItem")
            .WithSummary("DELETE /api/v1/items/{id}")
            .WithDescription("Soft deletes an item by setting its status to deleted.");

        return endpoints;
    }
}
