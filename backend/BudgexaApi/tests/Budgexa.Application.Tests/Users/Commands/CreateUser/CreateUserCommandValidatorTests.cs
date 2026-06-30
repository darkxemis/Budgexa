namespace Budgexa.Application.Tests.Users.Commands.CreateUser;

using Budgexa.Application.Users.Commands.CreateUser;
using Budgexa.Application.Users.DTOs;
using FluentValidation.TestHelper;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    private static CreateUserCommand ValidCommand() =>
        new(new UserCreateDto(
            "user@example.com",
            "Strong#Pass1",
            "Ada",
            "Lovelace",
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid() }));

    [Fact]
    public void Valid_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Email_WhenInvalid_HasError(string email)
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { Email = email } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("lowercase1!")]
    [InlineData("UPPERCASE1!")]
    [InlineData("NoDigits!")]
    [InlineData("NoSpecial1")]
    public void Password_WhenWeak_HasError(string password)
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { Password = password } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Password);
    }

    [Fact]
    public void EmptyFirstName_HasError()
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { FirstName = string.Empty } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.FirstName);
    }

    [Fact]
    public void EmptyLastName_HasError()
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { LastName = string.Empty } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.LastName);
    }

    [Fact]
    public void EmptyLanguageId_HasError()
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { LanguageId = Guid.Empty } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.LanguageId);
    }

    [Fact]
    public void EmptyRoles_HasError()
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { RoleIds = new List<Guid>() } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.RoleIds);
    }
}
