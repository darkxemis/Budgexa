namespace Budgexa.Infrastructure.BackgroundServices;

using Budgexa.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Periodically purges refresh tokens that are past their <c>ExpiresAt</c> timestamp
/// using a single bulk <c>DELETE</c> via <see cref="EntityFrameworkQueryableExtensions.ExecuteDeleteAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>.
/// Tokens stay in the table after rotation (kept for audit) but become useless once expired.
/// </summary>
internal sealed class RefreshTokenCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<MaintenanceSettings> options,
    ILogger<RefreshTokenCleanupService> logger
) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(
        Math.Max(1, options.Value.RefreshTokenCleanupIntervalMinutes));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "RefreshTokenCleanupService started. Interval: {Interval}.",
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
                logger.LogError(ex, "Refresh token cleanup sweep failed. Will retry on next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = DateTime.UtcNow;

        var deleted = await db.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            logger.LogInformation(
                "Purged {Count} expired refresh tokens (cutoff: {Cutoff:o}).",
                deleted,
                cutoff);
        }
    }
}
