namespace Budgexa.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IApplicationDbContext db
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
            var languageId = await db.Languages
                .AsNoTracking()
                .Where(l => l.Code == languageCode)
                .Select(l => (Guid?)l.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (languageId.HasValue)
            {
                return languageId.Value;
            }
        }

        // If not authenticated, return empty
        if (UserId == Guid.Empty)
            return Guid.Empty;

        // Fall back to language ID from database
        var userLanguageId = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == UserId)
            .Select(u => (Guid?)u.LanguageId)
            .FirstOrDefaultAsync(cancellationToken);

        return userLanguageId ?? Guid.Empty;
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