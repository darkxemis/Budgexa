namespace Budgexa.Application.Common.Interfaces;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Status> Statuses { get; }
    DbSet<StatusTranslation> StatusTranslations { get; }
    DbSet<Language> Languages { get; }
    DbSet<LanguageTranslation> LanguageTranslations { get; }
    DbSet<Company> Companies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
