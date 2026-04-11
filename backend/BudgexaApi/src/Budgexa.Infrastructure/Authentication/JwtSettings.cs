namespace Budgexa.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpirationInMinutes { get; init; }
}
