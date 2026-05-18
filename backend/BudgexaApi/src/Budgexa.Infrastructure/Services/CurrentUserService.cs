namespace Budgexa.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository
) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
        }
    }

    public Guid CompanyId
    {
        get
        {
            var companyIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue("company_id");
            return string.IsNullOrEmpty(companyIdClaim) ? Guid.Empty : Guid.Parse(companyIdClaim);
        }
    }

    public async Task<Guid> GetLanguageIdAsync(CancellationToken cancellationToken = default)
    {
        if (UserId == Guid.Empty)
            return Guid.Empty;

        var user = await userRepository.GetByIdAsync(UserId, cancellationToken);
        return user?.LanguageId ?? Guid.Empty;
    }

    public string Email
    {
        get
        {
            return httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles
    {
        get
        {
            return httpContextAccessor.HttpContext?.User?.FindAll("role").Select(c => c.Value) ?? Enumerable.Empty<string>();
        }
    }
}