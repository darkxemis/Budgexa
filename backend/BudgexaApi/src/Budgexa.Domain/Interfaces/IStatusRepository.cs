using Budgexa.Domain.Entities;

namespace Budgexa.Domain.Interfaces;

public interface IStatusRepository
{
    Task<Status?> GetByValueAsync(int value);
}
