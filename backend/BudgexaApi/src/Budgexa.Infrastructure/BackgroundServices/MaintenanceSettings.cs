namespace Budgexa.Infrastructure.BackgroundServices;

/// <summary>
/// Settings that control the cadence of background maintenance jobs
/// (refresh token cleanup, account lockout sweep, ...).
/// </summary>
public sealed class MaintenanceSettings
{
    public const string SectionName = "Maintenance";

    /// <summary>
    /// Interval in minutes between refresh token cleanup sweeps.
    /// </summary>
    public int RefreshTokenCleanupIntervalMinutes { get; init; } = 60;

    /// <summary>
    /// Interval in minutes between expired-lockout cleanup sweeps.
    /// </summary>
    public int LockoutCleanupIntervalMinutes { get; init; } = 60;
}
