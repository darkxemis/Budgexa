namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;

public class ItemTests
{
    private static Item ValidItem(Guid? id = null) =>
        Item.Create(
            companyId: Guid.NewGuid(),
            statusId: Guid.NewGuid(),
            sku: "SKU-001",
            name: "Test Item",
            description: "A test item",
            type: ItemType.Service,
            unit: "hour",
            unitPrice: 100m,
            taxRate: 21m,
            currency: "EUR",
            createdByUserId: Guid.NewGuid(),
            id: id);

    [Fact]
    public void Create_WithValidArguments_InitializesProperties()
    {
        var creatorId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var statusId = Guid.NewGuid();

        var item = Item.Create(
            companyId,
            statusId,
            "SKU-001",
            "Hourly Consulting",
            "Consulting service",
            ItemType.Service,
            "hour",
            120m,
            21m,
            "EUR",
            creatorId);

        item.Id.Should().NotBe(Guid.Empty);
        item.CompanyId.Should().Be(companyId);
        item.StatusId.Should().Be(statusId);
        item.Sku.Should().Be("SKU-001");
        item.Name.Should().Be("Hourly Consulting");
        item.Description.Should().Be("Consulting service");
        item.Type.Should().Be(ItemType.Service);
        item.Unit.Should().Be("hour");
        item.UnitPrice.Should().Be(120m);
        item.TaxRate.Should().Be(21m);
        item.Currency.Should().Be("EUR");
        item.CreatedByUserId.Should().Be(creatorId);
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        item.UpdatedAt.Should().BeNull();
        item.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var item = ValidItem(id);

        item.Id.Should().Be(id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankName_ThrowsArgumentException(string? name)
    {
        var act = () => Item.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SKU",
            name!,
            null,
            ItemType.Product,
            "unit",
            10m,
            21m,
            "EUR",
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankUnit_ThrowsArgumentException(string? unit)
    {
        var act = () => Item.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SKU",
            "Name",
            null,
            ItemType.Product,
            unit!,
            10m,
            21m,
            "EUR",
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_ThrowsArgumentException()
    {
        var act = () => Item.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SKU",
            "Name",
            null,
            ItemType.Product,
            "unit",
            -1m,
            21m,
            "EUR",
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_WithOutOfRangeTaxRate_ThrowsArgumentException(decimal taxRate)
    {
        var act = () => Item.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SKU",
            "Name",
            null,
            ItemType.Product,
            "unit",
            10m,
            taxRate,
            "EUR",
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Create_WithInvalidCurrency_ThrowsArgumentException(string currency)
    {
        var act = () => Item.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SKU",
            "Name",
            null,
            ItemType.Product,
            "unit",
            10m,
            21m,
            currency,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidArguments_ReplacesEditableFields()
    {
        var item = ValidItem();
        var updaterId = Guid.NewGuid();

        item.Update(
            "SKU-002",
            "Renamed",
            "New desc",
            ItemType.Product,
            "unit",
            55m,
            10m,
            "USD",
            updaterId);

        item.Sku.Should().Be("SKU-002");
        item.Name.Should().Be("Renamed");
        item.Description.Should().Be("New desc");
        item.Type.Should().Be(ItemType.Product);
        item.Unit.Should().Be("unit");
        item.UnitPrice.Should().Be(55m);
        item.TaxRate.Should().Be(10m);
        item.Currency.Should().Be("USD");
        item.UpdatedByUserId.Should().Be(updaterId);
        item.UpdatedAt.Should().NotBeNull();
        item.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsDeleted_ChangesStatusAndStampsUpdater()
    {
        var item = ValidItem();
        var deleteStatusId = Guid.NewGuid();
        var updaterId = Guid.NewGuid();

        item.MarkAsDeleted(deleteStatusId, updaterId);

        item.StatusId.Should().Be(deleteStatusId);
        item.UpdatedByUserId.Should().Be(updaterId);
        item.UpdatedAt.Should().NotBeNull();
    }
}
