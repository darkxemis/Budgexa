namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class CompanyTests
{
    [Fact]
    public void Create_WithValidArguments_InitializesProperties()
    {
        var creatorId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var company = Company.Create("Acme", "Description", startDate, null, creatorId);

        company.Id.Should().NotBe(Guid.Empty);
        company.Name.Should().Be("Acme");
        company.Description.Should().Be("Description");
        company.StartDate.Should().Be(startDate);
        company.EndDate.Should().BeNull();
        company.CreatedByUserId.Should().Be(creatorId);
        company.UpdatedAt.Should().BeNull();
        company.UpdatedByUserId.Should().BeNull();
        company.Users.Should().BeEmpty();
        company.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var company = Company.Create("Acme", null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid(), id);

        company.Id.Should().Be(id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankName_ThrowsArgumentException(string? name)
    {
        var act = () => Company.Create(name!, null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidName_ReplacesEditableFields()
    {
        var company = Company.Create("Acme", null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid());
        var newEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        var updaterId = Guid.NewGuid();

        company.Update("Acme Renamed", "New desc", newEnd, updaterId);

        company.Name.Should().Be("Acme Renamed");
        company.Description.Should().Be("New desc");
        company.EndDate.Should().Be(newEnd);
        company.UpdatedByUserId.Should().Be(updaterId);
        company.UpdatedAt.Should().NotBeNull();
        company.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_WithBlankName_ThrowsArgumentException(string? name)
    {
        var company = Company.Create("Acme", null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid());

        var act = () => company.Update(name!, null, null, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsContractValid_WhenNoEndDate_ReturnsTrue()
    {
        var company = Company.Create("Acme", null, DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid());

        company.IsContractValid().Should().BeTrue();
    }

    [Fact]
    public void IsContractValid_WhenEndDateInFuture_ReturnsTrue()
    {
        var company = Company.Create(
            "Acme",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Guid.NewGuid());

        company.IsContractValid().Should().BeTrue();
    }

    [Fact]
    public void IsContractValid_WhenEndDateInPast_ReturnsFalse()
    {
        var company = Company.Create(
            "Acme",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            Guid.NewGuid());

        company.IsContractValid().Should().BeFalse();
    }

    [Fact]
    public void IsContractValid_WhenEndDateIsToday_ReturnsTrue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var company = Company.Create("Acme", null, today, today, Guid.NewGuid());

        company.IsContractValid().Should().BeTrue();
    }
}
