namespace Budgexa.API.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Application.Users.DTOs;
using Budgexa.Application.Users.Queries.GetCurrentUser;
using MediatR;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/users").WithTags("Users");

        group.MapGet("/me",
            async (
                ISender sender,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
                var result = await sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<UserProfileResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetCurrentUser")
            .WithSummary("GET /api/v1/users/me")
            .WithDescription("Returns the profile of the currently authenticated user. Requires a valid JWT token in the Authorization header.");

        return endpoints;
    }
}