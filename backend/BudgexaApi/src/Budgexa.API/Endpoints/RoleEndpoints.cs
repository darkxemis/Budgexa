namespace Budgexa.API.Endpoints;

using Budgexa.API.Mappings;
using Budgexa.Application.Roles.Commands.CreateRole;
using Budgexa.Application.Roles.Commands.DeleteRole;
using Budgexa.Application.Roles.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
            .RequireAuthorization(new AuthorizeAttribute { Roles = "administrator,superadministrator" })
            .Produces<List<RoleDto>>(StatusCodes.Status200OK)
            .WithName("GetAllRoles")
            .WithSummary("GET /api/v1/roles")
            .WithDescription("Returns all roles. Only accessible to administrator and superadministrator.");

        group.MapPost("/",
            async (
                ISender sender,
                [FromBody] CreateRoleCommand command,
                CancellationToken cancellationToken) =>
            {
                var roleId = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/v1/roles/{roleId}", new { roleId });
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "superadministrator" })
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("CreateRole")
            .WithSummary("POST /api/v1/roles")
            .WithDescription("Creates a new role. Only accessible to superadministrator.");

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
            .RequireAuthorization(new AuthorizeAttribute { Roles = "superadministrator" })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("UpdateRole")
            .WithSummary("PUT /api/v1/roles/{id}")
            .WithDescription("Updates a role. Only accessible to superadministrator.");

        group.MapDelete("/{id:guid}",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteRoleCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "superadministrator" })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteRole")
            .WithSummary("DELETE /api/v1/roles/{id}")
            .WithDescription("Deletes a role. Only accessible to superadministrator.");

        return endpoints;
    }
}