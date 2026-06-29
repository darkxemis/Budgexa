namespace Budgexa.Application.Tests.Roles.Commands.UpdateRole;

using Budgexa.Application.Roles.Commands.UpdateRole;
using Budgexa.Application.Roles.DTOs;
using FluentValidation.TestHelper;

public class UpdateRoleCommandValidatorTests
{
    private readonly UpdateRoleCommandValidator _validator = new();

    [Fact]
    public void Valid_HasNoErrors()
    {
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleDto("manager"));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyId_HasError()
    {
        var command = new UpdateRoleCommand(Guid.Empty, new UpdateRoleDto("manager"));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void EmptyName_HasError()
    {
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleDto(string.Empty));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Name);
    }

    [Fact]
    public void TooLongName_HasError()
    {
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleDto(new string('a', 101)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Name);
    }
}
