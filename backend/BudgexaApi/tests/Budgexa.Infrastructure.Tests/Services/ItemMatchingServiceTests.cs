namespace Budgexa.Infrastructure.Tests.Services;

using Budgexa.Application.Common.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using Budgexa.Infrastructure.Persistence;
using Budgexa.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

public sealed class ItemMatchingServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ItemMatchingService _service;
    private readonly Guid _companyId = Guid.NewGuid();

    public ItemMatchingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new ItemMatchingService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var items = new List<Item>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                Name = "Ventana De Aluminio",
                UnitPrice = 150.00m,
                TaxRate = 21.00m,
                Unit = "Unidad",
                UnitMeasure = UnitMeasure.Unit,
                StatusId = StatusIds.Active,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                Name = "Puerta De Madera",
                UnitPrice = 200.00m,
                TaxRate = 21.00m,
                Unit = "Unidad",
                UnitMeasure = UnitMeasure.Unit,
                StatusId = StatusIds.Active,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                Name = "Consultoría Técnica Avanzada",
                UnitPrice = 75.00m,
                TaxRate = 21.00m,
                Unit = "Hora",
                UnitMeasure = UnitMeasure.Hour,
                StatusId = StatusIds.Active,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                Name = "Deleted Item",
                UnitPrice = 999.00m,
                TaxRate = 21.00m,
                Unit = "Unidad",
                UnitMeasure = UnitMeasure.Unit,
                StatusId = StatusIds.Delete, // Should be ignored
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Items.AddRange(items);
        _context.SaveChanges();
    }

    [Fact]
    public async Task MatchItemsAsync_WithExactMatch_ReturnsResolvedItem()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("Ventana De Aluminio", 4, null)
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var matched = result[0];
        Assert.NotNull(matched.ItemId);
        Assert.Equal("Ventana De Aluminio", matched.ProductName);
        Assert.Equal(4, matched.Quantity);
        Assert.Equal(150.00m, matched.UnitPrice);
        Assert.Equal(21.00m, matched.TaxRate);
        Assert.Equal("Unidad", matched.Unit);
        Assert.Equal((int)UnitMeasure.Unit, matched.UnitMeasure);
    }

    [Fact]
    public async Task MatchItemsAsync_WithFuzzyMatch_ReturnsResolvedItem()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("ventana aluminio", 2, null) // Fuzzy match
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var matched = result[0];
        Assert.NotNull(matched.ItemId);
        Assert.Equal("Ventana De Aluminio", matched.ProductName);
    }

    [Fact]
    public async Task MatchItemsAsync_WithAccents_ReturnsResolvedItem()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("Consultoria Tecnica Avanzada", 3, null) // Without accents
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var matched = result[0];
        Assert.NotNull(matched.ItemId);
        Assert.Equal("Consultoría Técnica Avanzada", matched.ProductName);
    }

    [Fact]
    public async Task MatchItemsAsync_WithDiscount_PreservesDiscount()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("Puerta De Madera", 1, 10.00m)
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var matched = result[0];
        Assert.NotNull(matched.ItemId);
        Assert.Equal(10.00m, matched.DiscountPercentage);
    }

    [Fact]
    public async Task MatchItemsAsync_WithNoMatch_ReturnsNullItemId()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("Producto Inexistente XYZ", 5, null)
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var matched = result[0];
        Assert.Null(matched.ItemId);
        Assert.Equal("Producto Inexistente XYZ", matched.ProductName);
        Assert.Equal(5, matched.Quantity);
        Assert.Null(matched.UnitPrice);
        Assert.Null(matched.TaxRate);
    }

    [Fact]
    public async Task MatchItemsAsync_WithDeletedItem_IgnoresDeletedItem()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("Deleted Item", 1, null)
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var matched = result[0];
        Assert.Null(matched.ItemId); // Should not match deleted items
    }

    [Fact]
    public async Task MatchItemsAsync_WithMultipleItems_ReturnsAllMatches()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("Ventana De Aluminio", 2, null),
            new("Puerta De Madera", 3, 5.00m),
            new("Producto Desconocido", 1, null)
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.NotNull(result[0].ItemId);
        Assert.NotNull(result[1].ItemId);
        Assert.Null(result[2].ItemId);
    }

    [Fact]
    public async Task MatchItemsAsync_IgnoresStopWords()
    {
        // Arrange
        var aiItems = new List<AiItemDto>
        {
            new("puerta de la madera", 1, null) // "de" and "la" are stop words
        };

        // Act
        var result = await _service.MatchItemsAsync(_companyId, aiItems, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var matched = result[0];
        Assert.NotNull(matched.ItemId);
        Assert.Equal("Puerta De Madera", matched.ProductName);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
