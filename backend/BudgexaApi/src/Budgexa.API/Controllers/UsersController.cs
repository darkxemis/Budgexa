namespace Budgexa.API.Controllers;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Application.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return Ok(result);
    }
}
