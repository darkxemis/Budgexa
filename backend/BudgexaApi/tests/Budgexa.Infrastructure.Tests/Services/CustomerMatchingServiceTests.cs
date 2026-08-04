namespace Budgexa.Infrastructure.Tests.Services;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Infrastructure.Persistence;
using Budgexa.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

public sealed class CustomerMatchingServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CustomerMatchingService _service;
    private readonly Guid _companyId = Guid.NewGuid();

    public CustomerMatchingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new CustomerMatchingService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                LegalName = "Acme Corporation",
                TaxId = "B12345678",
                StatusId = StatusIds.Active,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                LegalName = "Tech Solutions SL",
                TradeName = "TechSol",
                TaxId = "A87654321",
                StatusId = StatusIds.Active,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                LegalName = "Consulting Services International",
                TaxId = "C11223344",
                StatusId = StatusIds.Active,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = _companyId,
                LegalName = "Deleted Customer",
                TaxId = "D99999999",
                StatusId = StatusIds.Delete, // Should be ignored
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Customers.AddRange(customers);
        _context.SaveChanges();
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithExactTaxId_ReturnsCustomerId()
    {
        // Arrange
        var taxId = "B12345678";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, null, taxId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaxId == taxId);
        Assert.Equal(customer!.Id, result);
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithTaxIdWithSpaces_ReturnsCustomerId()
    {
        // Arrange
        var taxIdWithSpaces = "B 12 34 56 78";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, null, taxIdWithSpaces, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithTaxIdWithHyphens_ReturnsCustomerId()
    {
        // Arrange
        var taxIdWithHyphens = "A-87654321";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, null, taxIdWithHyphens, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithMatchingLegalName_ReturnsCustomerId()
    {
        // Arrange
        var customerName = "Acme Corporation";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, customerName, null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.LegalName == customerName);
        Assert.Equal(customer!.Id, result);
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithFuzzyNameMatch_ReturnsCustomerId()
    {
        // Arrange
        var fuzzyName = "tech solutions"; // Should match "Tech Solutions SL"

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, fuzzyName, null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithTradeName_ReturnsCustomerId()
    {
        // Arrange
        var tradeName = "TechSol";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, tradeName, null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TradeName == tradeName);
        Assert.Equal(customer!.Id, result);
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithNoMatch_ReturnsNull()
    {
        // Arrange
        var nonExistentName = "Nonexistent Company XYZ";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, nonExistentName, null, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindCustomerIdAsync_WithDeletedCustomerTaxId_ReturnsNull()
    {
        // Arrange
        var deletedTaxId = "D99999999";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, null, deletedTaxId, CancellationToken.None);

        // Assert
        Assert.Null(result); // Deleted customers should be ignored
    }

    [Fact]
    public async Task FindCustomerIdAsync_PrioritizesTaxIdOverName()
    {
        // Arrange
        var taxId = "B12345678";
        var nameOfDifferentCustomer = "Tech Solutions SL";

        // Act
        var result = await _service.FindCustomerIdAsync(_companyId, nameOfDifferentCustomer, taxId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaxId == "B12345678");
        Assert.Equal(customer!.Id, result); // Should match by TaxId, not by name
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
