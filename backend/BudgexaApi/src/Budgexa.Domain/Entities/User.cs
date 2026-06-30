namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;

public sealed class User : Entity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid LanguageId { get; private set; }
    public Guid StatusId { get; private set; }

    public Company Company { get; private set; } = default!;
    public Language Language { get; private set; } = default!;
    public Status Status { get; private set; } = default!;

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { }

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        Guid companyId,
        Guid languageId,
        Guid statusId)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            CompanyId = companyId,
            LanguageId = languageId,
            StatusId = statusId
        };
    }

    public void Update(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        Guid companyId,
        Guid languageId)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        CompanyId = companyId;
        LanguageId = languageId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRoles(List<Guid> roleIds)
    {
        UserRoles.Clear();
        foreach (var roleId in roleIds)
        {
            UserRoles.Add(UserRole.Create(Id, roleId));
        }
    }

    public void MarkAsDeleted(Guid deletedStatusId)
    {
        StatusId = deletedStatusId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
            FailedLoginAttempts = 0;
        }
    }

    public void ResetLoginFailures()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public bool IsLockedOut() => LockoutEnd is { } end && end > DateTime.UtcNow;
}
