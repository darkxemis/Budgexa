namespace Budgexa.API.Controllers;

using Budgexa.API.Middleware;
using Budgexa.Application.Auth.Commands.RefreshToken;
using Budgexa.Application.Auth.Commands.Register;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Application.Auth.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Authentication endpoints: registration, login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController(ISender sender, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// POST /api/v1/auth/register
    /// </summary>
    /// <remarks>
    /// Registers a new user in the system.
    /// </remarks>
    /// <param name="command">Registration data (email, password, first name, last name).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the newly created user.</returns>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Validation failed — check metadata for field errors.</response>
    /// <response code="409">A user with this email already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var userId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = userId }, new { userId });
    }

    /// <summary>
    /// POST /api/v1/auth/login
    /// </summary>
    /// <remarks>
    /// Authenticates a user and sets access token and refresh token as HTTP-only cookies.
    /// </remarks>
    /// <param name="query">User credentials (email and password).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Basic user information. Tokens are set as HTTP-only cookies.</returns>
    /// <response code="200">Login successful — tokens set in cookies.</response>
    /// <response code="400">Validation failed — check metadata for field errors.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        SetTokenCookies(result.Token, result.RefreshToken);
        return Ok(new LoginResponse(result.UserId, result.Email, result.FullName));
    }

    /// <summary>
    /// POST /api/v1/auth/refresh
    /// </summary>
    /// <remarks>
    /// Refreshes the access token using the refresh token cookie. The old refresh token is revoked and a new one is issued (token rotation).
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Basic user information. New tokens are set as HTTP-only cookies.</returns>
    /// <response code="200">Tokens refreshed successfully.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var result = await sender.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
        SetTokenCookies(result.Token, result.RefreshToken);
        return Ok(new LoginResponse(result.UserId, result.Email, result.FullName));
    }

    /// <summary>
    /// POST /api/v1/auth/logout
    /// </summary>
    /// <remarks>
    /// Clears both authentication cookies, effectively logging out the user.
    /// </remarks>
    /// <response code="204">Logged out successfully.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        var expiredOptions = CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(-1));
        Response.Cookies.Delete("accessToken", expiredOptions);
        Response.Cookies.Delete("refreshToken", expiredOptions);
        return NoContent();
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var accessMinutes = configuration.GetValue<int>("JwtSettings:ExpirationInMinutes");
        var refreshDays = configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays");

        Response.Cookies.Append("accessToken", accessToken, CreateCookieOptions(
            DateTimeOffset.UtcNow.AddMinutes(accessMinutes)));

        Response.Cookies.Append("refreshToken", refreshToken, CreateCookieOptions(
            DateTimeOffset.UtcNow.AddDays(refreshDays)));
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
