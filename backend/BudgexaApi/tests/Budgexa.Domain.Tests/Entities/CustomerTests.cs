namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class CustomerTests
{
    private static Customer ValidCustomer(Guid? id = null) =>
        Customer.Create(
            companyId: Guid.NewGuid(),
            statusId: Guid.NewGuid(),
            legalName: "Acme S.L.",
            tradeName: "Acme",
            taxId: "B12345678",
            email: "billing@acme.test",
            phone: "+34 600 000 000",
            addressLine: "Calle Falsa 123",
            city: "Madrid",
            postalCode: "28001",
            province: "Madrid",
            country: "ES",
            notes: "VIP",
            createdByUserId: Guid.NewGuid(),
            id: id);

    [Fact]
    public void Create_WithValidArguments_InitializesProperties()
    {
        var creatorId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var statusId = Guid.NewGuid();

        var customer = Customer.Create(
            companyId,
            statusId,
            "Acme S.L.",
            "Acme",
            "B12345678",
            "billing@acme.test",
            "+34 600 000 000",
            "Calle Falsa 123",
            "Madrid",
            "28001",
            "Madrid",
            "ES",
            "VIP",
            creatorId);

        customer.Id.Should().NotBe(Guid.Empty);
        customer.CompanyId.Should().Be(companyId);
        customer.StatusId.Should().Be(statusId);
        customer.LegalName.Should().Be("Acme S.L.");
        customer.TradeName.Should().Be("Acme");
        customer.TaxId.Should().Be("B12345678");
        customer.Email.Should().Be("billing@acme.test");
        customer.Phone.Should().Be("+34 600 000 000");
        customer.AddressLine.Should().Be("Calle Falsa 123");
        customer.City.Should().Be("Madrid");
        customer.PostalCode.Should().Be("28001");
        customer.Province.Should().Be("Madrid");
        customer.Country.Should().Be("ES");
        customer.Notes.Should().Be("VIP");
        customer.CreatedByUserId.Should().Be(creatorId);
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        customer.UpdatedAt.Should().BeNull();
        customer.UpdatedByUserId.Should().BeNull();
        customer.Budgets.Should().BeEmpty();
        customer.Invoices.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var customer = ValidCustomer(id);

        customer.Id.Should().Be(id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankLegalName_ThrowsArgumentException(string? legalName)
    {
        var act = () => Customer.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            legalName!,
            null,
            "B12345678",
            null, null, null, null, null, null, null, null,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankTaxId_ThrowsArgumentException(string? taxId)
    {
        var act = () => Customer.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Acme S.L.",
            null,
            taxId!,
            null, null, null, null, null, null, null, null,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidArguments_ReplacesEditableFields()
    {
        var customer = ValidCustomer();
        var updaterId = Guid.NewGuid();

        customer.Update(
            "Acme Renamed S.L.",
            "Acme R",
            "B87654321",
            "new@acme.test",
            "+34 611 111 111",
            "New Address 1",
            "Barcelona",
            "08001",
            "Barcelona",
            "ES",
            "Renamed notes",
            updaterId);

        customer.LegalName.Should().Be("Acme Renamed S.L.");
        customer.TradeName.Should().Be("Acme R");
        customer.TaxId.Should().Be("B87654321");
        customer.Email.Should().Be("new@acme.test");
        customer.Phone.Should().Be("+34 611 111 111");
        customer.AddressLine.Should().Be("New Address 1");
        customer.City.Should().Be("Barcelona");
        customer.PostalCode.Should().Be("08001");
        customer.Province.Should().Be("Barcelona");
        customer.Country.Should().Be("ES");
        customer.Notes.Should().Be("Renamed notes");
        customer.UpdatedByUserId.Should().Be(updaterId);
        customer.UpdatedAt.Should().NotBeNull();
        customer.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithBlankLegalName_ThrowsArgumentException(string? legalName)
    {
        var customer = ValidCustomer();

        var act = () => customer.Update(
            legalName!,
            null,
            "B12345678",
            null, null, null, null, null, null, null, null,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithBlankTaxId_ThrowsArgumentException(string? taxId)
    {
        var customer = ValidCustomer();

        var act = () => customer.Update(
            "Acme",
            null,
            taxId!,
            null, null, null, null, null, null, null, null,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAsDeleted_ChangesStatusAndStampsUpdater()
    {
        var customer = ValidCustomer();
        var deleteStatusId = Guid.NewGuid();
        var updaterId = Guid.NewGuid();

        customer.MarkAsDeleted(deleteStatusId, updaterId);

        customer.StatusId.Should().Be(deleteStatusId);
        customer.UpdatedByUserId.Should().Be(updaterId);
        customer.UpdatedAt.Should().NotBeNull();
        customer.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
