namespace Budgexa.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { }

    internal Role(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Role Create(string name, Guid? id = null)
    {
        return new Role(id ?? Guid.NewGuid(), name);
    }

    public void Update(string newName)
    {
        Name = newName;
    }
}
