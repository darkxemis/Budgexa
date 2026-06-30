namespace Budgexa.Application.Tests.TestHelpers;

using Budgexa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Builds isolated <see cref="ApplicationDbContext"/> instances backed by the EF Core
/// in-memory provider. Each call returns a fresh database so tests stay independent.
/// </summary>
internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }
}
