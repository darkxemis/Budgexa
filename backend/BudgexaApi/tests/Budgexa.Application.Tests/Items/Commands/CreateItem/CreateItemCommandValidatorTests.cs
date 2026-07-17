namespace Budgexa.Application.Tests.Items.Commands.CreateItem;

using Budgexa.Application.Items.Commands.CreateItem;
using Budgexa.Application.Items.DTOs;
using Budgexa.Domain.Enums;
using FluentValidation.TestHelper;

public class CreateItemCommandValidatorTests
{
    private readonly CreateItemCommandValidator _validator = new();

    private static CreateItemCommand Valid(
        string? sku = "SKU-001",
        string name = "Name",
        string? description = null,
        ItemType type = ItemType.Service,
        string unit = "hour",
        decimal unitPrice = 100m,
        decimal taxRate = 21m,
        string currency = "EUR")
    {
        return new CreateItemCommand(new ItemCreateDto(sku, name, description, type, unit, unitPrice, taxRate, currency));
    }

    [Fact]
    public void DefaultValid_HasNoErrors()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_WhenEmpty_HasError(string name)
    {
        var result = _validator.TestValidate(Valid(name: name));

        result.ShouldHaveValidationErrorFor(x => x.Dto.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Currency_WhenInvalid_HasError(string currency)
    {
        var result = _validator.TestValidate(Valid(currency: currency));

        result.ShouldHaveValidationErrorFor(x => x.Dto.Currency);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void UnitPrice_WhenNegative_HasError(decimal price)
    {
        var result = _validator.TestValidate(Valid(unitPrice: price));

        result.ShouldHaveValidationErrorFor(x => x.Dto.UnitPrice);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void TaxRate_WhenOutOfRange_HasError(decimal rate)
    {
        var result = _validator.TestValidate(Valid(taxRate: rate));

        result.ShouldHaveValidationErrorFor(x => x.Dto.TaxRate);
    }

    [Fact]
    public void Sku_WhenNull_IsAllowed()
    {
        var result = _validator.TestValidate(Valid(sku: null));

        result.ShouldNotHaveValidationErrorFor(x => x.Dto.Sku);
    }

    [Fact]
    public void Sku_WhenTooLong_HasError()
    {
        var result = _validator.TestValidate(Valid(sku: new string('s', 101)));

        result.ShouldHaveValidationErrorFor(x => x.Dto.Sku);
    }
}
