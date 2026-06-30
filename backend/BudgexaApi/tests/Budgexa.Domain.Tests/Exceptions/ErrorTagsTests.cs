namespace Budgexa.Domain.Tests.Exceptions;

using Budgexa.Domain.Exceptions;

public class ErrorTagsTests
{
    [Fact]
    public void Auth_TagsHaveExpectedValues()
    {
        ErrorTags.Auth.InvalidCredentials.Should().Be("auth.invalidCredentials");
        ErrorTags.Auth.InvalidRefreshToken.Should().Be("auth.invalidRefreshToken");
        ErrorTags.Auth.AccountLocked.Should().Be("auth.accountLocked");
        ErrorTags.Auth.ContractExpired.Should().Be("auth.contractExpired");
    }

    [Fact]
    public void User_TagsHaveExpectedValues()
    {
        ErrorTags.User.NotFound.Should().Be("user.notFound");
        ErrorTags.User.EmailAlreadyExists.Should().Be("user.emailAlreadyExists");
    }

    [Fact]
    public void Role_TagsHaveExpectedValues()
    {
        ErrorTags.Role.NotFound.Should().Be("role.notFound");
        ErrorTags.Role.NameExists.Should().Be("role.nameExists");
    }

    [Fact]
    public void Status_TagsHaveExpectedValues()
    {
        ErrorTags.Status.NotFound.Should().Be("status.notFound");
    }

    [Fact]
    public void Server_TagsHaveExpectedValues()
    {
        ErrorTags.Server.InternalError.Should().Be("server.internalError");
    }

    [Fact]
    public void Validation_TagsHaveExpectedValues()
    {
        ErrorTags.Validation.Failed.Should().Be("validation.failed");
    }
}
