namespace Budgexa.API.Endpoints;

using Budgexa.API.Mappings;
using Budgexa.API.Middleware;
using Budgexa.Application.Roles.Commands.CreateRole;
using Budgexa.Application.Roles.Commands.DeleteRole;
using Budgexa.Application.Roles.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/roles").WithTags("Roles");

        group.MapGet("/",
            async (
            ISender sender, 
            CancellationToken cancellationToken) =>
            {
                var roles = await sender.Send(new GetAllRolesQuery(), cancellationToken);
                return Results.Ok(roles);
            })
            .RequireAuthorization("AdminOnly")
            .Produces<List<RoleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetAllRoles")
            .WithSummary("GET /api/v1/roles")
            .WithDescription("Returns all roles.");

        group.MapPost("/",
            async (
                ISender sender,
                [FromBody] CreateRoleCommand command,
                CancellationToken cancellationToken) =>
            {
                var roleId = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/v1/roles/{roleId}", new { roleId });
            })
            .RequireAuthorization("SuperAdminOnly")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("CreateRole")
            .WithSummary("POST /api/v1/roles")
            .WithDescription("Creates a new role.");

        group.MapPut("/{id:guid}",
            async (
                Guid id,
                [FromBody] UpdateRoleDto request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(id);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization("SuperAdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("UpdateRole")
            .WithSummary("PUT /api/v1/roles/{id}")
            .WithDescription("Updates a role.");

        group.MapDelete("/{id:guid}",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteRoleCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization("SuperAdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DeleteRole")
            .WithSummary("DELETE /api/v1/roles/{id}")
            .WithDescription("Deletes a role.");

        return endpoints;
    }
}