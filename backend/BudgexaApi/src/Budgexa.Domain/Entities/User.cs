namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;

public sealed class User : Entity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName
        };
    }
}
