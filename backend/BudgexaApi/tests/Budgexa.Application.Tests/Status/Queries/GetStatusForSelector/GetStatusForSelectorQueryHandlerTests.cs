namespace Budgexa.Application.Tests.Status.Queries.GetStatusForSelector;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Status.Queries.GetStatusForSelector;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using BudgexaStatus = Budgexa.Domain.Entities.Status;
using NSubstitute;

public class GetStatusForSelectorQueryHandlerTests
{
    [Fact]
    public async Task Handle_NoGroup_ReturnsAllStatuses()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);

        var sut = new GetStatusForSelectorQueryHandler(db, current);

        var result = await sut.Handle(new GetStatusForSelectorQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithGroup_FiltersResults()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);
        db.Statuses.Add(BudgexaStatus.Create("Project", "Active", 10));
        await db.SaveChangesAsync();

        var current = Substitute.For<ICurrentUserService>();
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);

        var sut = new GetStatusForSelectorQueryHandler(db, current);

        var result = await sut.Handle(new GetStatusForSelectorQuery(Group: "User"), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().NotContain(s => s.Name == "Active");
    }

    [Fact]
    public async Task Handle_WithSearch_FiltersByName()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);

        var sut = new GetStatusForSelectorQueryHandler(db, current);

        var result = await sut.Handle(new GetStatusForSelectorQuery(SearchQuery: "Del"), CancellationToken.None);

        result.Should().ContainSingle(s => s.Name == "Deleted");
    }
}
