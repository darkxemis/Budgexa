namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Users.Commands.CreateUser;
using Budgexa.Application.Users.Commands.DeleteUser;
using Budgexa.Application.Users.Commands.UpdateCurrentUser;
using Budgexa.Application.Users.Commands.UpdateUser;
using Budgexa.Application.Users.DTOs;
using Budgexa.Application.Users.Queries.GetAllUsers;
using Budgexa.Application.Users.Queries.GetCurrentUser;
using Budgexa.Application.Users.Queries.GetUserById;
using MediatR;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/users").WithTags("Users");

        group.MapGet("/me",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<UserProfileResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetCurrentUser")
            .WithSummary("GET /api/v1/users/me")
            .WithDescription("Returns the profile of the currently authenticated user.");

        group.MapPatch("/me",
            async (UpdateCurrentUserDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new UpdateCurrentUserCommand(dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<UserProfileResult>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("UpdateCurrentUser")
            .WithSummary("PUT /api/v1/users/me")
            .WithDescription("Updates the profile of the currently authenticated user. Only firstName, lastName, password and language can be modified.");

        group.MapGet("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization("AdminOnly")
                .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetUserById")
            .WithSummary("GET /api/v1/users/{id}")
            .WithDescription("Returns a specific user by ID.");

        group.MapGet("/",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetAllUsersQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization("AdminOnly")
            .Produces<IEnumerable<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetAllUsers")
            .WithSummary("GET /api/v1/users")
            .WithDescription("Returns all users from the authenticated user's company.");

        group.MapPost("/",
            async (UserCreateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateUserCommand(dto), cancellationToken);
                return Results.Created($"/api/v1/users/{result.Id}", result);
            })
            .RequireAuthorization("AdminOnly")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("CreateUser")
            .WithSummary("POST /api/v1/users")
            .WithDescription("Creates a new user and returns the created user profile.");

        group.MapPut("/{id:guid}",
            async (Guid id, UserUpdateDto dto, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new UpdateUserCommand(id, dto), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization("AdminOnly")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("UpdateUser")
            .WithSummary("PUT /api/v1/users/{id}")
            .WithDescription("Updates an existing user and returns the updated user profile.");

        group.MapDelete("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteUserCommand(id), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("DeleteUser")
            .WithSummary("DELETE /api/v1/users/{id}")
            .WithDescription("Soft deletes a user by changing its status to deleted.");

        return endpoints;
    }
}