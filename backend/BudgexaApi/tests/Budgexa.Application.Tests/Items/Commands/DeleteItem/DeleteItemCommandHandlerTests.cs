namespace Budgexa.Application.Tests.Items.Commands.DeleteItem;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.Commands.DeleteItem;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class DeleteItemCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        return current;
    }

    [Fact]
    public async Task Handle_UnknownItem_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new DeleteItemCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteItemCommand(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Item.NotFound);
    }

    [Fact]
    public async Task Handle_ItemFromOtherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var foreign = TestDataSeeder.SeedItem(db, Guid.NewGuid(), newStatusId, sku: "SKU-X");

        var sut = new DeleteItemCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteItemCommand(foreign.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingItem_MarksAsDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var item = TestDataSeeder.SeedItem(db, companyId, newStatusId, sku: "SKU-A");

        var sut = new DeleteItemCommandHandler(db, BuildCurrentUser(companyId));

        await sut.Handle(new DeleteItemCommand(item.Id), CancellationToken.None);

        db.Items.Single(i => i.Id == item.Id).StatusId.Should().Be(StatusIds.Delete);
    }
}
