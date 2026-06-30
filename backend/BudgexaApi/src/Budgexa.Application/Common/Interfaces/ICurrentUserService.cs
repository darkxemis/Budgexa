namespace Budgexa.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid CompanyId { get; }
    Task<Guid> GetLanguageIdAsync(CancellationToken cancellationToken = default);
    string Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
}