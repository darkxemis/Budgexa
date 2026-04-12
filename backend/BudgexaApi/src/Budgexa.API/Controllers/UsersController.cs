namespace Budgexa.API.Controllers;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Application.Users.DTOs;
using Budgexa.Application.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// User profile endpoints. Requires authentication.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/users/me
    /// </summary>
    /// <remarks>
    /// Returns the profile of the currently authenticated user.
    /// Requires a valid JWT token in the Authorization header.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user's profile data.</returns>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="401">Unauthorized — a valid JWT token is required.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("me")]
    [ProducesResponseType<UserProfileResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return Ok(result);
    }
}
