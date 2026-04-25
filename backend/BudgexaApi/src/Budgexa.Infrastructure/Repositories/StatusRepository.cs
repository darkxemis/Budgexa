namespace Budgexa.Infrastructure.Repositories;

using Budgexa.Domain.Entities;
using Budgexa.Domain.Interfaces;
using Budgexa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class StatusRepository(
    ApplicationDbContext dbContext
) : IStatusRepository
{
    public async Task<Status?> GetByValueAsync(int value)
    {
        return await dbContext.Statuses.FirstOrDefaultAsync(s => s.Value == value);
    }
}
