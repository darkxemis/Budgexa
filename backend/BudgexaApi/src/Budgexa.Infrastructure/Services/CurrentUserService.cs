namespace Budgexa.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository,
    ILanguageRepository languageRepository
) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var userIdClaim = User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
        }
    }

    public Guid CompanyId
    {
        get
        {
            var companyIdClaim = User?.FindFirstValue("company_id");
            return string.IsNullOrEmpty(companyIdClaim) ? Guid.Empty : Guid.Parse(companyIdClaim);
        }
    }

    public async Task<Guid> GetLanguageIdAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;

        // Check if language code is provided in the request header (highest priority)
        var languageCode = httpContext?.Request.Headers["X-Language-Code"].FirstOrDefault();
        if (!string.IsNullOrEmpty(languageCode))
        {
            var language = await languageRepository.GetByCodeAsync(languageCode, cancellationToken);
            if (language != null)
            {
                return language.Id;
            }
        }

        // If not authenticated, return empty
        if (UserId == Guid.Empty)
            return Guid.Empty;

        // Fall back to language ID from database
        var user = await userRepository.GetByIdAsync(UserId, cancellationToken);
        return user?.LanguageId ?? Guid.Empty;
    }

    public string Email
    {
        get
        {
            return User?.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles
    {
        get
        {
            return User?.FindAll("role").Select(c => c.Value) ?? Enumerable.Empty<string>();
        }
    }
}