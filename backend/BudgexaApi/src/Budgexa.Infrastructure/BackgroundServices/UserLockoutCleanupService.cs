namespace Budgexa.Infrastructure.BackgroundServices;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Periodically clears expired account lockouts using a single bulk <c>UPDATE</c>
/// via <see cref="EntityFrameworkQueryableExtensions.ExecuteUpdateAsync"/>.
/// This complements the lazy reset performed in the login flow: users whose
/// lockout window has passed are normalized to <c>LockoutEnd = null</c> and
/// <c>FailedLoginAttempts = 0</c> without waiting for them to attempt a login.
/// </summary>
internal sealed class UserLockoutCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<MaintenanceSettings> options,
    ILogger<UserLockoutCleanupService> logger
) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(
        Math.Max(1, options.Value.LockoutCleanupIntervalMinutes));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "UserLockoutCleanupService started. Interval: {Interval}.",
            _interval);

        using var timer = new PeriodicTimer(_interval);

        do
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User lockout cleanup sweep failed. Will retry on next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var now = DateTime.UtcNow;

        var updated = await db.Users
            .Where(u =>
                u.LockoutEnd != null
                && u.LockoutEnd < now
                && u.StatusId != StatusIds.Delete)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.LockoutEnd, (DateTime?)null)
                .SetProperty(u => u.FailedLoginAttempts, 0),
                cancellationToken);

        if (updated > 0)
        {
            logger.LogInformation(
                "Cleared {Count} expired account lockouts (cutoff: {Cutoff:o}).",
                updated,
                now);
        }
    }
}
