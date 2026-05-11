namespace Budgexa.Infrastructure.Repositories;

using Budgexa.Domain.Entities;
using Budgexa.Domain.Interfaces;
using Budgexa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class StatusRepository(
    ApplicationDbContext dbContext
) : IStatusRepository
{
    public async Task<Status?> GetByValueAsync(int value, CancellationToken cancellationToken = default)
    {
        return await dbContext.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Value == value, cancellationToken);
    }
}
