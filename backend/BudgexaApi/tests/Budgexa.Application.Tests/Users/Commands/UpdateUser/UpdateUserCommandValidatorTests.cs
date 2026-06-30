namespace Budgexa.Application.Tests.Users.Commands.UpdateUser;

using Budgexa.Application.Users.Commands.UpdateUser;
using Budgexa.Application.Users.DTOs;
using FluentValidation.TestHelper;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator = new();

    private static UpdateUserCommand ValidCommand() =>
        new(Guid.NewGuid(), new UserUpdateDto(
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

    [Fact]
    public void EmptyId_HasError()
    {
        var command = ValidCommand() with { Id = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void InvalidEmail_HasError(string email)
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
    public void WeakPassword_HasError(string password)
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { Password = password } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Password);
    }

    [Fact]
    public void EmptyRoles_HasError()
    {
        var command = ValidCommand() with { Dto = ValidCommand().Dto with { RoleIds = new List<Guid>() } };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.RoleIds);
    }
}
