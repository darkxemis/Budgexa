namespace Budgexa.Application.Tests.Items.Queries.GetItemById;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.Queries.GetItemById;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class GetItemByIdQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    [Fact]
    public async Task Handle_UnknownItem_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new GetItemByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetItemByIdQuery(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Item.NotFound);
    }

    [Fact]
    public async Task Handle_DeletedItem_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        var item = TestDataSeeder.SeedItem(db, companyId, deleteStatusId);

        var sut = new GetItemByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetItemByIdQuery(item.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ItemInOtherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var foreign = TestDataSeeder.SeedItem(db, Guid.NewGuid(), newStatusId);

        var sut = new GetItemByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetItemByIdQuery(foreign.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingItem_ReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var item = TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Hourly", sku: "SKU-1");

        var sut = new GetItemByIdQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetItemByIdQuery(item.Id), CancellationToken.None);

        result.Id.Should().Be(item.Id);
        result.Name.Should().Be("Hourly");
        result.Sku.Should().Be("SKU-1");
        result.CompanyId.Should().Be(companyId);
    }
}
