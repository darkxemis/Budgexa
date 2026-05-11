namespace Budgexa.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid CompanyId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}