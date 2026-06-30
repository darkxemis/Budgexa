namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class UserTests
{
    private const string Email = "user@example.com";
    private const string PasswordHash = "hashed";
    private const string FirstName = "Ada";
    private const string LastName = "Lovelace";

    [Fact]
    public void Create_WithValidArguments_InitializesProperties()
    {
        var companyId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        var statusId = Guid.NewGuid();

        var user = User.Create(Email, PasswordHash, FirstName, LastName, companyId, languageId, statusId);

        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(Email);
        user.PasswordHash.Should().Be(PasswordHash);
        user.FirstName.Should().Be(FirstName);
        user.LastName.Should().Be(LastName);
        user.CompanyId.Should().Be(companyId);
        user.LanguageId.Should().Be(languageId);
        user.StatusId.Should().Be(statusId);
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.UpdatedAt.Should().BeNull();
        user.UserRoles.Should().BeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_ReplacesEditableFieldsAndStampsUpdatedAt()
    {
        var user = CreateUser();
        var newCompanyId = Guid.NewGuid();
        var newLanguageId = Guid.NewGuid();

        user.Update("new@example.com", "new-hash", "Grace", "Hopper", newCompanyId, newLanguageId);

        user.Email.Should().Be("new@example.com");
        user.PasswordHash.Should().Be("new-hash");
        user.FirstName.Should().Be("Grace");
        user.LastName.Should().Be("Hopper");
        user.CompanyId.Should().Be(newCompanyId);
        user.LanguageId.Should().Be(newLanguageId);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SetRoles_ReplacesExistingUserRoles()
    {
        var user = CreateUser();
        var firstRole = Guid.NewGuid();
        var secondRole = Guid.NewGuid();
        user.SetRoles(new List<Guid> { firstRole });

        var newRole = Guid.NewGuid();
        user.SetRoles(new List<Guid> { secondRole, newRole });

        user.UserRoles.Should().HaveCount(2);
        user.UserRoles.Select(ur => ur.RoleId).Should().BeEquivalentTo(new[] { secondRole, newRole });
        user.UserRoles.Should().OnlyContain(ur => ur.UserId == user.Id);
    }

    [Fact]
    public void SetRoles_WithEmptyList_ClearsRoles()
    {
        var user = CreateUser();
        user.SetRoles(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

        user.SetRoles(new List<Guid>());

        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsDeleted_UpdatesStatusAndTimestamp()
    {
        var user = CreateUser();
        var deletedStatusId = Guid.NewGuid();

        user.MarkAsDeleted(deletedStatusId);

        user.StatusId.Should().Be(deletedStatusId);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RegisterFailedLogin_BelowThreshold_IncrementsCounterWithoutLockout()
    {
        var user = CreateUser();

        user.RegisterFailedLogin(maxAttempts: 5, lockoutDuration: TimeSpan.FromMinutes(15));

        user.FailedLoginAttempts.Should().Be(1);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void RegisterFailedLogin_AtThreshold_TriggersLockoutAndResetsCounter()
    {
        var user = CreateUser();
        var lockoutDuration = TimeSpan.FromMinutes(15);

        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLogin(maxAttempts: 5, lockoutDuration: lockoutDuration);
        }

        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd!.Value.Should().BeCloseTo(DateTime.UtcNow.Add(lockoutDuration), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ResetLoginFailures_ClearsCounterAndLockout()
    {
        var user = CreateUser();
        user.RegisterFailedLogin(maxAttempts: 100, lockoutDuration: TimeSpan.FromMinutes(15));

        user.ResetLoginFailures();

        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutEndIsFuture_ReturnsTrue()
    {
        var user = CreateUser();
        user.RegisterFailedLogin(maxAttempts: 1, lockoutDuration: TimeSpan.FromMinutes(15));

        user.IsLockedOut().Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutEndIsNull_ReturnsFalse()
    {
        var user = CreateUser();

        user.IsLockedOut().Should().BeFalse();
    }

    private static User CreateUser() =>
        User.Create(Email, PasswordHash, FirstName, LastName, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
}
