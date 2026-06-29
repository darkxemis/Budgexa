namespace Budgexa.Application.Tests.Auth.Queries.Login;

using Budgexa.Application.Auth.Queries.Login;
using FluentValidation.TestHelper;

public class LoginQueryValidatorTests
{
    private readonly LoginQueryValidator _validator = new();

    [Fact]
    public void ValidQuery_HasNoErrors()
    {
        var query = new LoginQuery("user@example.com", "anything");

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Email_WhenInvalid_HasError(string email)
    {
        var query = new LoginQuery(email, "anything");

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Password_WhenEmpty_HasError()
    {
        var query = new LoginQuery("user@example.com", string.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
