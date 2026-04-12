namespace Budgexa.API.Controllers;

using Budgexa.Application.Auth.Commands.Register;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Application.Auth.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Authentication endpoints: registration and login.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController(ISender sender) : ControllerBase
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
    [HttpPost("register")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var userId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = userId }, new { userId });
    }

    /// <summary>
    /// POST /api/v1/auth/login
    /// </summary>
    /// <remarks>
    /// Authenticates a user and returns a JWT token.
    /// </remarks>
    /// <param name="query">User credentials (email and password).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JWT token along with basic user information.</returns>
    /// <response code="200">Login successful.</response>
    [HttpPost("login")]
    [ProducesResponseType<AuthResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
