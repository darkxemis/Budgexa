namespace Budgexa.Application.Tests.Roles.Commands.CreateRole;

using Budgexa.Application.Roles.Commands.CreateRole;
using Budgexa.Application.Roles.DTOs;
using FluentValidation.TestHelper;

public class CreateRoleCommandValidatorTests
{
    private readonly CreateRoleCommandValidator _validator = new();

    [Fact]
    public void Valid_HasNoErrors()
    {
        var command = new CreateRoleCommand(new RoleCreateDto("manager"));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyName_HasError()
    {
        var command = new CreateRoleCommand(new RoleCreateDto(string.Empty));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Name);
    }

    [Fact]
    public void TooLongName_HasError()
    {
        var command = new CreateRoleCommand(new RoleCreateDto(new string('a', 101)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Name);
    }
}
