namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Customers.Commands.CreateCustomer;
using Budgexa.Application.Customers.Commands.DeleteCustomer;
using Budgexa.Application.Customers.Commands.UpdateCustomer;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Application.Customers.Queries.GetAllCustomers;
using Budgexa.Application.Customers.Queries.GetCustomerById;
using Budgexa.Application.Customers.Queries.GetCustomersForSelector;
using Budgexa.Application.Customers.Queries.GetCustomersGrid;
using MediatR;

public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/customers").WithTags("Customers");

        group.MapGet("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetCustomerById")
            .WithSummary("GET /api/v1/customers/{id}")
            .WithDescription("Returns a specific customer of the authenticated user's company by id.");

        group.MapGet("/",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAllCustomersQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<IEnumerable<CustomerDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetAllCustomers")
            .WithSummary("GET /api/v1/customers")
            .WithDescription("Returns all non-deleted customers from the authenticated user's company.");

        group.MapPost("/grid",
            async (GridRequestDto request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCustomersGridQuery(request), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<GridResponseDto<CustomerGridDto>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetCustomersGrid")
            .WithSummary("POST /api/v1/customers/grid")
            .WithDescription("Returns a paginated, filtered, and sorted grid of customers with support for advanced filtering, ordering, and global search.");

        group.MapGet("/selector",
            async (string? searchQuery, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCustomersForSelectorQuery(searchQuery), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<List<SelectorDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetCustomersForSelector")
            .WithSummary("GET /api/v1/customers/selector")
            .WithDescription("Returns customers for selector usage with optional search filter.");

        group.MapPost("/",
            async (CustomerCreateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateCustomerCommand(dto), cancellationToken);
                return Results.Created($"/api/v1/customers/{result.Id}", result);
            })
            .RequireAuthorization()
            .Produces<CustomerDto>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("CreateCustomer")
            .WithSummary("POST /api/v1/customers")
            .WithDescription("Creates a new customer in the authenticated user's company.");

        group.MapPut("/{id:guid}",
            async (Guid id, CustomerUpdateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new UpdateCustomerCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("UpdateCustomer")
            .WithSummary("PUT /api/v1/customers/{id}")
            .WithDescription("Updates an existing customer.");

        group.MapDelete("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteCustomerCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DeleteCustomer")
            .WithSummary("DELETE /api/v1/customers/{id}")
            .WithDescription("Soft deletes a customer by setting its status to deleted.");

        return endpoints;
    }
}
