namespace Budgexa.Application.Tests.Auth.Commands.Register;

using Budgexa.Application.Auth.Commands.Register;
using FluentValidation.TestHelper;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("user@example.com", "Strong#Pass1", "Ada", "Lovelace", Guid.NewGuid(), Guid.NewGuid(), new[] { Guid.NewGuid() });

    [Fact]
    public void ValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Email_WhenInvalid_HasError(string email)
    {
        var command = ValidCommand() with { Email = email };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("lowercase1!")]
    [InlineData("UPPERCASE1!")]
    [InlineData("NoDigits!")]
    [InlineData("NoSpecial1")]
    [InlineData("")]
    public void Password_WhenWeak_HasError(string password)
    {
        var command = ValidCommand() with { Password = password };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void FirstName_WhenEmpty_HasError()
    {
        var command = ValidCommand() with { FirstName = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void FirstName_WhenTooLong_HasError()
    {
        var command = ValidCommand() with { FirstName = new string('a', 101) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void LastName_WhenEmpty_HasError()
    {
        var command = ValidCommand() with { LastName = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
