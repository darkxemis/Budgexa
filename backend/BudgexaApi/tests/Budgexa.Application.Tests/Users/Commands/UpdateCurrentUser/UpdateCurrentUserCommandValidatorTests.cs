namespace Budgexa.Application.Tests.Users.Commands.UpdateCurrentUser;

using Budgexa.Application.Users.Commands.UpdateCurrentUser;
using Budgexa.Application.Users.DTOs;
using FluentValidation.TestHelper;

public class UpdateCurrentUserCommandValidatorTests
{
    private readonly UpdateCurrentUserCommandValidator _validator = new();

    private static UpdateCurrentUserCommand ValidCommand(string password = "Strong#Pass1") =>
        new(new UpdateCurrentUserDto("Ada", "Lovelace", password, Guid.NewGuid()));

    [Fact]
    public void Valid_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
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

    [Theory]
    [InlineData("short")]
    [InlineData("lowercase1!")]
    [InlineData("UPPERCASE1!")]
    [InlineData("NoDigits!")]
    [InlineData("NoSpecial1")]
    public void WeakPassword_HasError(string password)
    {
        var command = ValidCommand(password);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Password);
    }
}
