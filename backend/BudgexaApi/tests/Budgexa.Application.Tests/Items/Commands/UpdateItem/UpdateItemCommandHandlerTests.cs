namespace Budgexa.Application.Tests.Items.Commands.UpdateItem;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.Commands.UpdateItem;
using Budgexa.Application.Items.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class UpdateItemCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static ItemUpdateDto BuildDto(string? sku = "SKU-001", string name = "Updated") =>
        new(sku, name, "desc", ItemType.Service, "hour", 120m, 21m, "EUR");

    [Fact]
    public async Task Handle_UnknownItem_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new UpdateItemCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateItemCommand(Guid.NewGuid(), BuildDto()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Item.NotFound);
    }

    [Fact]
    public async Task Handle_ItemFromOtherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var foreign = TestDataSeeder.SeedItem(db, Guid.NewGuid(), newStatusId, sku: "SKU-OTHER");

        var sut = new UpdateItemCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateItemCommand(foreign.Id, BuildDto()), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_DeletedItem_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        var item = TestDataSeeder.SeedItem(db, companyId, deleteStatusId, sku: "SKU-DEL");

        var sut = new UpdateItemCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateItemCommand(item.Id, BuildDto()), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, newStatusId, sku: "SKU-A", name: "A");
        var item = TestDataSeeder.SeedItem(db, companyId, newStatusId, sku: "SKU-B", name: "B");

        var sut = new UpdateItemCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateItemCommand(item.Id, BuildDto(sku: "SKU-A")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Item.SkuAlreadyExists);
    }

    [Fact]
    public async Task Handle_KeepingSameSku_DoesNotConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var item = TestDataSeeder.SeedItem(db, companyId, newStatusId, sku: "SKU-A");

        var sut = new UpdateItemCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new UpdateItemCommand(item.Id, BuildDto(sku: "SKU-A", name: "Renamed")), CancellationToken.None);

        result.Name.Should().Be("Renamed");
        result.Sku.Should().Be("SKU-A");
    }

    [Fact]
    public async Task Handle_ValidUpdate_PersistsAndReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var item = TestDataSeeder.SeedItem(db, companyId, newStatusId, sku: "SKU-A", name: "Old");

        var sut = new UpdateItemCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new UpdateItemCommand(item.Id, BuildDto(sku: "SKU-B", name: "New")), CancellationToken.None);

        result.Id.Should().Be(item.Id);
        result.Name.Should().Be("New");
        result.Sku.Should().Be("SKU-B");
        db.Items.Single(i => i.Id == item.Id).Sku.Should().Be("SKU-B");
    }
}
