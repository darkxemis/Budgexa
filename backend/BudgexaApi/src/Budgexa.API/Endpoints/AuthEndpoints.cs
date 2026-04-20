namespace Budgexa.API.Endpoints;

using Budgexa.API.Middleware;
using Budgexa.Application.Auth.Commands.RefreshToken;
using Budgexa.Application.Auth.Commands.Register;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Application.Auth.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register",
            async (
                ISender sender,
                [FromBody] RegisterCommand command,
                CancellationToken cancellationToken) =>
            {
                var userId = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/v1/auth/register/{userId}", new { userId });
            })
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .WithName("Register")
            .WithSummary("POST /api/v1/auth/register")
            .WithDescription("Registers a new user in the system.");

        group.MapPost("/login",
            async (
                ISender sender,
                [FromBody] LoginQuery query,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(query, cancellationToken);

                var config = http.RequestServices.GetRequiredService<IConfiguration>();
                var accessMinutes = config.GetValue<int>("JwtSettings:ExpirationInMinutes");
                var refreshDays = config.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays");

                http.Response.Cookies.Append("accessToken", result.Token, CreateCookieOptions(DateTimeOffset.UtcNow.AddMinutes(accessMinutes)));
                http.Response.Cookies.Append("refreshToken", result.RefreshToken, CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(refreshDays)));

                return Results.Ok(new LoginResponse(result.UserId, result.Email, result.FullName));
            })
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("Login")
            .WithSummary("POST /api/v1/auth/login")
            .WithDescription("Authenticates a user and sets access token and refresh token as HTTP-only cookies.");

        group.MapPost("/refresh",
            async (
                ISender sender,
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                var refreshToken = http.Request.Cookies["refreshToken"];
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return Results.Unauthorized();
                }

                var result = await sender.Send(new RefreshTokenCommand(refreshToken), cancellationToken);

                var config = http.RequestServices.GetRequiredService<IConfiguration>();
                var accessMinutes = config.GetValue<int>("JwtSettings:ExpirationInMinutes");
                var refreshDays = config.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays");

                http.Response.Cookies.Append("accessToken", result.Token, CreateCookieOptions(DateTimeOffset.UtcNow.AddMinutes(accessMinutes)));
                http.Response.Cookies.Append("refreshToken", result.RefreshToken, CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(refreshDays)));

                return Results.Ok(new LoginResponse(result.UserId, result.Email, result.FullName));
            })
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .WithName("Refresh")
            .WithSummary("POST /api/v1/auth/refresh")
            .WithDescription("Refreshes the access token using the refresh token cookie. The old refresh token is revoked and a new one is issued (token rotation).");

        group.MapPost("/logout",
            (HttpContext http) =>
            {
                var expiredOptions = CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(-1));
                http.Response.Cookies.Delete("accessToken", expiredOptions);
                http.Response.Cookies.Delete("refreshToken", expiredOptions);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .WithName("Logout")
            .WithSummary("POST /api/v1/auth/logout")
            .WithDescription("Clears both authentication cookies, effectively logging out the user.");

        return endpoints;
    }

    private static CookieOptions CreateCookieOptions(DateTimeOffset expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = expires,
    };
}