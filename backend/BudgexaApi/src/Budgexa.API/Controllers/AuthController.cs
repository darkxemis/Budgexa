namespace Budgexa.API.Controllers;

using Budgexa.Application.Auth.Commands.Register;
using Budgexa.Application.Auth.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
