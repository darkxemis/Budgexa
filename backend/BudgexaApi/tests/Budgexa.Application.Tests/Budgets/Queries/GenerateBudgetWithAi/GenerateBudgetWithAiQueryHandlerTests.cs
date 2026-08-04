namespace Budgexa.Application.Tests.Budgets.Queries.GenerateBudgetWithAi;

using System.Text.Json;
using Budgexa.Application.Budgets.Queries.GenerateBudgetWithAi;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Common.Services;
using Budgexa.Application.PublicBudgets.DTOs;
using Budgexa.Application.PublicBudgets.Services;
using Budgexa.Domain.Exceptions;
using Moq;

public sealed class GenerateBudgetWithAiQueryHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICustomerMatchingService> _customerMatchingServiceMock = new();
    private readonly Mock<IItemMatchingService> _itemMatchingServiceMock = new();
    private readonly Mock<IDateParsingService> _dateParsingServiceMock = new();
    private readonly GenerateBudgetWithAiQueryHandler _handler;
    private readonly Guid _companyId = Guid.NewGuid();

    public GenerateBudgetWithAiQueryHandlerTests()
    {
        _handler = new GenerateBudgetWithAiQueryHandler(
            _aiServiceMock.Object,
            _currentUserServiceMock.Object,
            _customerMatchingServiceMock.Object,
            _itemMatchingServiceMock.Object,
            _dateParsingServiceMock.Object);

        _currentUserServiceMock.Setup(x => x.CompanyId).Returns(_companyId);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsCompleteResponse()
    {
        // Arrange
        var userRequest = "Presupuesto para Acme Corp, 3 ventanas de aluminio";
        var customerId = Guid.NewGuid();

        var aiData = new AiPrivateBudgetDto(
            CustomerName: "Acme Corp",
            CustomerTaxId: "B12345678",
            Number: "PRE-2025-001",
            IssueDate: "2025-01-15",
            ValidUntil: "2025-02-15",
            Currency: "EUR",
            Notes: "Test notes",
            TermsAndConditions: "Test terms",
            Items: new List<AiItemDto> { new("Ventana De Aluminio", 3, null) }
        );

        var aiJsonResponse = JsonSerializer.Serialize(aiData);

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BudgetItemsAiResult(aiJsonResponse, userRequest, "llama3.2"));

        _customerMatchingServiceMock
            .Setup(x => x.FindCustomerIdAsync(_companyId, "Acme Corp", "B12345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerId);

        _dateParsingServiceMock.Setup(x => x.ParseDate("2025-01-15")).Returns(new DateOnly(2025, 1, 15));
        _dateParsingServiceMock.Setup(x => x.ParseDate("2025-02-15")).Returns(new DateOnly(2025, 2, 15));

        var matchedItems = new List<MatchedItemDto>
        {
            new(Guid.NewGuid(), "Ventana De Aluminio", 3, null, 150.00m, 21.00m, "Unidad", 1)
        };

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchedItems);

        var query = new GenerateBudgetWithAiQuery(new PrivateBudgetAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userRequest, result.OriginalRequest);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("Acme Corp", result.CustomerName);
        Assert.Equal("B12345678", result.CustomerTaxId);
        Assert.Equal("PRE-2025-001", result.Number);
        Assert.Equal(new DateOnly(2025, 1, 15), result.IssueDate);
        Assert.Equal(new DateOnly(2025, 2, 15), result.ValidUntil);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal("Test notes", result.Notes);
        Assert.Equal("Test terms", result.TermsAndConditions);
        Assert.Single(result.Items);
        Assert.Equal("llama3.2", result.Model);
    }

    [Fact]
    public async Task Handle_WithCustomerNotFound_ReturnsNullCustomerId()
    {
        // Arrange
        var userRequest = "Presupuesto para cliente desconocido";

        var aiData = new AiPrivateBudgetDto(
            CustomerName: "Unknown Customer",
            CustomerTaxId: null,
            Number: null,
            IssueDate: null,
            ValidUntil: null,
            Currency: null,
            Notes: null,
            TermsAndConditions: null,
            Items: new List<AiItemDto>()
        );

        var aiJsonResponse = JsonSerializer.Serialize(aiData);

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiJsonResult(aiJsonResponse, userRequest, "llama3.2"));

        _customerMatchingServiceMock
            .Setup(x => x.FindCustomerIdAsync(_companyId, "Unknown Customer", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchedItemDto>());

        var query = new GenerateBudgetWithAiQuery(new PrivateBudgetAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result.CustomerId);
        Assert.Equal("Unknown Customer", result.CustomerName);
    }

    [Fact]
    public async Task Handle_WithInvalidDates_ReturnsNullDates()
    {
        // Arrange
        var userRequest = "Presupuesto sin fechas válidas";

        var aiData = new AiPrivateBudgetDto(
            CustomerName: null,
            CustomerTaxId: null,
            Number: null,
            IssueDate: "invalid date",
            ValidUntil: "another invalid",
            Currency: null,
            Notes: null,
            TermsAndConditions: null,
            Items: new List<AiItemDto>()
        );

        var aiJsonResponse = JsonSerializer.Serialize(aiData);

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiJsonResult(aiJsonResponse, userRequest, "llama3.2"));

        _dateParsingServiceMock.Setup(x => x.ParseDate("invalid date")).Returns((DateOnly?)null);
        _dateParsingServiceMock.Setup(x => x.ParseDate("another invalid")).Returns((DateOnly?)null);

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchedItemDto>());

        var query = new GenerateBudgetWithAiQuery(new PrivateBudgetAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result.IssueDate);
        Assert.Null(result.ValidUntil);
    }

    [Fact]
    public async Task Handle_WithMalformedJson_ThrowsAppException()
    {
        // Arrange
        var userRequest = "Request that returns invalid JSON";

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiJsonResult("{invalid json", userRequest, "llama3.2"));

        var query = new GenerateBudgetWithAiQuery(new PrivateBudgetAiRequestDto(userRequest));

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithItemsMatched_ReturnsMatchedItems()
    {
        // Arrange
        var userRequest = "3 ventanas y 2 puertas";

        var aiData = new AiPrivateBudgetDto(
            CustomerName: null,
            CustomerTaxId: null,
            Number: null,
            IssueDate: null,
            ValidUntil: null,
            Currency: null,
            Notes: null,
            TermsAndConditions: null,
            Items: new List<AiItemDto>
            {
                new("Ventana De Aluminio", 3, null),
                new("Puerta De Madera", 2, 5.00m)
            }
        );

        var aiJsonResponse = JsonSerializer.Serialize(aiData);

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiJsonResult(aiJsonResponse, userRequest, "llama3.2"));

        var matchedItems = new List<MatchedItemDto>
        {
            new(Guid.NewGuid(), "Ventana De Aluminio", 3, null, 150.00m, 21.00m, "Unidad", 1),
            new(Guid.NewGuid(), "Puerta De Madera", 2, 5.00m, 200.00m, 21.00m, "Unidad", 1)
        };

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchedItems);

        var query = new GenerateBudgetWithAiQuery(new PrivateBudgetAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5.00m, result.Items[1].DiscountPercentage);
    }
}
