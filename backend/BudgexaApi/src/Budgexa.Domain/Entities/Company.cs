using Budgexa.Domain.Common;

namespace Budgexa.Domain.Entities;

public sealed class Company : Entity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public ICollection<User> Users { get; private set; } = new List<User>();

    private Company() { }

    internal Company(
        Guid id,
        string name,
        string? description,
        DateTime startDate,
        DateTime? endDate,
        Guid createdByUserId)
    {
        Id = id;
        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Company Create(
        string name,
        string? description,
        DateTime startDate,
        DateTime? endDate,
        Guid createdByUserId,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company name cannot be empty.");

        return new Company(id ?? Guid.NewGuid(), name, description, startDate, endDate, createdByUserId);
    }

    public void Update(
        string name,
        string? description,
        DateTime? endDate,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company name cannot be empty.");

        Name = name;
        Description = description;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }
}