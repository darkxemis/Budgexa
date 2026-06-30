namespace Budgexa.Application.Tests.Languages.Queries.GetLanguagesForSelector;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Languages.Queries.GetLanguagesForSelector;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetLanguagesForSelectorQueryHandlerTests
{
    [Fact]
    public async Task Handle_NoSearch_ReturnsAllLanguages()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);

        var sut = new GetLanguagesForSelectorQueryHandler(db, current);

        var result = await sut.Handle(new GetLanguagesForSelectorQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithSearch_FiltersInMemoryCaseInsensitively()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);

        var sut = new GetLanguagesForSelectorQueryHandler(db, current);

        var result = await sut.Handle(new GetLanguagesForSelectorQuery("eng"), CancellationToken.None);

        result.Should().ContainSingle(s => s.Name == "English");
    }
}
