namespace Budgexa.Application.Tests.Common.Helpers;

using Budgexa.Application.Common.Helpers;
using Microsoft.EntityFrameworkCore;

public class GridQueryExtensionsTests
{
    public sealed class Item
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class ItemContext : DbContext
    {
        public ItemContext(DbContextOptions<ItemContext> options) : base(options)
        {
        }

        public DbSet<Item> Items => this.Set<Item>();
    }

    private static ItemContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ItemContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ItemContext(options);
    }

    private static async Task<ItemContext> CreateContextWithItemsAsync(int count)
    {
        var context = CreateContext();
        context.Items.AddRange(Enumerable.Range(1, count).Select(i => new Item { Id = i, Name = $"Item-{i}" }));
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task ToGridResponseAsync_AppliesPagingAndComputesTotalPages()
    {
        await using var context = await CreateContextWithItemsAsync(25);

        var response = await context.Items.OrderBy(i => i.Id).ToGridResponseAsync(page: 2, pageSize: 10, totalCount: 25);

        response.Data.Should().HaveCount(10);
        response.Data.First().Id.Should().Be(11);
        response.Data.Last().Id.Should().Be(20);
        response.TotalCount.Should().Be(25);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task ToGridResponseAsync_LastPagePartial_ReturnsRemainingItems()
    {
        await using var context = await CreateContextWithItemsAsync(25);

        var response = await context.Items.OrderBy(i => i.Id).ToGridResponseAsync(page: 3, pageSize: 10, totalCount: 25);

        response.Data.Should().HaveCount(5);
        response.Data.First().Id.Should().Be(21);
        response.Data.Last().Id.Should().Be(25);
        response.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task ToGridResponseAsync_PageSizeZero_TotalPagesZero()
    {
        await using var context = await CreateContextWithItemsAsync(5);

        var response = await context.Items.OrderBy(i => i.Id).ToGridResponseAsync(page: 1, pageSize: 0, totalCount: 5);

        response.Data.Should().BeEmpty();
        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ToGridResponseAsync_EmptySource_ReturnsEmptyPage()
    {
        await using var context = CreateContext();

        var response = await context.Items.OrderBy(i => i.Id).ToGridResponseAsync(page: 1, pageSize: 10, totalCount: 0);

        response.Data.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.TotalPages.Should().Be(0);
    }
}

