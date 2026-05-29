namespace Budgexa.Domain.Interfaces;

using Budgexa.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailForUpdateAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> Users, int TotalCount)> GetGridAsync(
        Guid companyId,
        Guid currentUserLanguageId,
        int page,
        int pageSize,
        string? sorting,
        string? filters,
        string? search,
        CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
}
