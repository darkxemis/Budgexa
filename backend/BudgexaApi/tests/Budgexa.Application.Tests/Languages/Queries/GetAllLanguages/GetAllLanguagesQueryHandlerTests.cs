namespace Budgexa.Application.Tests.Languages.Queries.GetAllLanguages;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Languages.Queries.GetAllLanguages;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetAllLanguagesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsLanguagesOrderedByName()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);

        var sut = new GetAllLanguagesQueryHandler(db, current);

        var result = await sut.Handle(new GetAllLanguagesQuery(), CancellationToken.None);

        result.Should().NotBeEmpty();
        result.Should().BeInAscendingOrder(l => l.Name);
        result.Should().Contain(l => l.Code == "en");
        result.Should().Contain(l => l.Code == "es");
    }
}
