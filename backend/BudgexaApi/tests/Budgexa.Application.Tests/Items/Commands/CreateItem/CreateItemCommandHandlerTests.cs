namespace Budgexa.Application.Tests.Items.Commands.CreateItem;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.Commands.CreateItem;
using Budgexa.Application.Items.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class CreateItemCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static ItemCreateDto BuildDto(string? sku = "SKU-001", string name = "Item") =>
        new(sku, name, "desc", ItemType.Service, "hour", 100m, 21m, "EUR");

    [Fact]
    public async Task Handle_NewItem_CreatesAndReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new CreateItemCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateItemCommand(BuildDto()), CancellationToken.None);

        result.Sku.Should().Be("SKU-001");
        result.Name.Should().Be("Item");
        result.CompanyId.Should().Be(companyId);
        result.StatusId.Should().Be(StatusIds.New);
        db.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_NoSku_AllowsCreation()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new CreateItemCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateItemCommand(BuildDto(sku: null)), CancellationToken.None);

        result.Sku.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DuplicateSkuInSameCompany_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, newStatusId, sku: "SKU-001");

        var sut = new CreateItemCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new CreateItemCommand(BuildDto(sku: "SKU-001")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Item.SkuAlreadyExists);
    }

    [Fact]
    public async Task Handle_SameSkuInDifferentCompany_IsAllowed()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, Guid.NewGuid(), newStatusId, sku: "SKU-001");

        var sut = new CreateItemCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateItemCommand(BuildDto(sku: "SKU-001")), CancellationToken.None);

        result.CompanyId.Should().Be(companyId);
        db.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_DuplicateSkuOnDeletedItem_IsAllowed()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, deleteStatusId, sku: "SKU-001");

        var sut = new CreateItemCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateItemCommand(BuildDto(sku: "SKU-001")), CancellationToken.None);

        result.Sku.Should().Be("SKU-001");
    }
}
