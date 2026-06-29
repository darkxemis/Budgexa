namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class UserRoleTests
{
    [Fact]
    public void Create_InitializesUserIdAndRoleId()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRole = UserRole.Create(userId, roleId);

        userRole.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(roleId);
    }
}
