namespace Budgexa.Infrastructure.Repositories;

using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Interfaces;
using Budgexa.Infrastructure.Persistence;
using Gridify;
using Microsoft.EntityFrameworkCore;

public sealed class UserRepository(
    ApplicationDbContext dbContext
) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Language)
            .Include(u => u.Company)
            .Include(u => u.Status)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Language)
            .Include(u => u.Status)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByEmailForUpdateAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Language)
            .Include(u => u.Status)
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.CompanyId == companyId)
            .Include(u => u.Language)
            .Include(u => u.Company)
            .Include(u => u.Status)
            .Where(u => u.Status.Value != (int)BaseStatus.Delete)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetGridAsync(
        Guid companyId,
        Guid currentUserLanguageId,
        int page,
        int pageSize,
        string? sorting,
        string? filters,
        string? search,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = dbContext.Users
            .AsNoTracking()
            .Where(u => u.CompanyId == companyId)
            .Where(u => u.Status.Value != (int)BaseStatus.Delete)
            .Include(u => u.Language)
                .ThenInclude(l => l.Translations)
            .Include(u => u.Company)
            .Include(u => u.Status)
                .ThenInclude(s => s.Translations)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Company.Name.ToLower().Contains(searchLower) ||
                u.Language.Name.ToLower().Contains(searchLower));
        }

        var mapper = GetUserGridMapper(currentUserLanguageId);

        if (!string.IsNullOrWhiteSpace(filters))
        {
            query = query.ApplyFiltering(filters, mapper);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(sorting))
        {
            query = query.ApplyOrdering(sorting, mapper);
        }
        else
        {
            query = query.OrderBy(u => u.CreatedAt);
        }

        var skip = (page - 1) * pageSize;
        query = query.Skip(skip).Take(pageSize);

        var users = await query.ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        dbContext.Users.Update(user);
    }

    public void Delete(User user)
    {
        dbContext.Users.Remove(user);
    }

    private static IGridifyMapper<User> GetUserGridMapper(Guid currentUserLanguageId)
    {
        var mapper = new GridifyMapper<User>()
            .GenerateMappings()
            .AddMap("Id", u => u.Id)
            .AddMap("Email", u => u.Email)
            .AddMap("FirstName", u => u.FirstName)
            .AddMap("LastName", u => u.LastName)
            .AddMap("CompanyId", u => u.CompanyId)
            .AddMap("CompanyName", u => u.Company.Name)
            .AddMap("LanguageId", u => u.LanguageId)
            .AddMap("LanguageName", u => u.Language.Translations
                .Where(t => t.TranslationLanguageId == currentUserLanguageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? u.Language.Name)
            .AddMap("StatusId", u => u.StatusId)
            .AddMap("StatusName", u => u.Status.Translations
                .Where(t => t.LanguageId == currentUserLanguageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? u.Status.Name)
            .AddMap("CreatedAt", u => u.CreatedAt)
            .AddMap("UpdatedAt", u => u.UpdatedAt);

        return mapper;
    }
}

