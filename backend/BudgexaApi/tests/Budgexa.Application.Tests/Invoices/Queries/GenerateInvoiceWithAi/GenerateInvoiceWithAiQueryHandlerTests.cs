namespace Budgexa.Application.Tests.Invoices.Queries.GenerateInvoiceWithAi;

using System.Text.Json;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Common.Services;
using Budgexa.Application.Invoices.Queries.GenerateInvoiceWithAi;
using Budgexa.Application.PublicBudgets.DTOs;
using Budgexa.Application.PublicBudgets.Services;
using Budgexa.Domain.Exceptions;
using Moq;

public sealed class GenerateInvoiceWithAiQueryHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICustomerMatchingService> _customerMatchingServiceMock = new();
    private readonly Mock<IItemMatchingService> _itemMatchingServiceMock = new();
    private readonly Mock<IDateParsingService> _dateParsingServiceMock = new();
    private readonly GenerateInvoiceWithAiQueryHandler _handler;
    private readonly Guid _companyId = Guid.NewGuid();

    public GenerateInvoiceWithAiQueryHandlerTests()
    {
        _handler = new GenerateInvoiceWithAiQueryHandler(
            _aiServiceMock.Object,
            _currentUserServiceMock.Object,
            _customerMatchingServiceMock.Object,
            _itemMatchingServiceMock.Object,
            _dateParsingServiceMock.Object);

        _currentUserServiceMock.Setup(x => x.GetCompanyId()).Returns(_companyId);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsCompleteResponse()
    {
        // Arrange
        var userRequest = "Factura para Acme Corp, 5 horas de consultoría";
        var customerId = Guid.NewGuid();

        var aiData = new AiPrivateInvoiceDto(
            CustomerName: "Acme Corp",
            CustomerTaxId: "B12345678",
            Series: "A",
            Number: "001",
            IssueDate: "2025-01-15",
            DueDate: "2025-02-15",
            Currency: "EUR",
            Notes: "Consultoría técnica avanzada",
            Items: new List<AiItemDto> { new("Consultoría Técnica", 5, null) }
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
            new(Guid.NewGuid(), "Consultoría Técnica", 5, null, 75.00m, 21.00m, "Hora", 2)
        };

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchedItems);

        var query = new GenerateInvoiceWithAiQuery(new PrivateInvoiceAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userRequest, result.OriginalRequest);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("Acme Corp", result.CustomerName);
        Assert.Equal("B12345678", result.CustomerTaxId);
        Assert.Equal("A", result.Series);
        Assert.Equal("001", result.Number);
        Assert.Equal(new DateOnly(2025, 1, 15), result.IssueDate);
        Assert.Equal(new DateOnly(2025, 2, 15), result.DueDate);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal("Consultoría técnica avanzada", result.Notes);
        Assert.Single(result.Items);
        Assert.Equal("llama3.2", result.Model);
    }

    [Fact]
    public async Task Handle_WithCustomerNotFound_ReturnsNullCustomerId()
    {
        // Arrange
        var userRequest = "Factura para cliente desconocido";

        var aiData = new AiPrivateInvoiceDto(
            CustomerName: "Unknown Customer",
            CustomerTaxId: null,
            Series: null,
            Number: null,
            IssueDate: null,
            DueDate: null,
            Currency: null,
            Notes: null,
            Items: new List<AiItemDto>()
        );

        var aiJsonResponse = JsonSerializer.Serialize(aiData);

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BudgetItemsAiResult(aiJsonResponse, userRequest, "llama3.2"));

        _customerMatchingServiceMock
            .Setup(x => x.FindCustomerIdAsync(_companyId, "Unknown Customer", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchedItemDto>());

        var query = new GenerateInvoiceWithAiQuery(new PrivateInvoiceAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result.CustomerId);
        Assert.Equal("Unknown Customer", result.CustomerName);
    }

    [Fact]
    public async Task Handle_WithSeriesAndNumber_ReturnsSeriesAndNumber()
    {
        // Arrange
        var userRequest = "Factura serie B número 042";

        var aiData = new AiPrivateInvoiceDto(
            CustomerName: null,
            CustomerTaxId: null,
            Series: "B",
            Number: "042",
            IssueDate: null,
            DueDate: null,
            Currency: null,
            Notes: null,
            Items: new List<AiItemDto>()
        );

        var aiJsonResponse = JsonSerializer.Serialize(aiData);

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BudgetItemsAiResult(aiJsonResponse, userRequest, "llama3.2"));

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchedItemDto>());

        var query = new GenerateInvoiceWithAiQuery(new PrivateInvoiceAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal("B", result.Series);
        Assert.Equal("042", result.Number);
    }

    [Fact]
    public async Task Handle_WithInvalidDueDate_ReturnsNullDueDate()
    {
        // Arrange
        var userRequest = "Factura con fecha de vencimiento inválida";

        var aiData = new AiPrivateInvoiceDto(
            CustomerName: null,
            CustomerTaxId: null,
            Series: null,
            Number: null,
            IssueDate: "2025-01-15",
            DueDate: "invalid date",
            Currency: null,
            Notes: null,
            Items: new List<AiItemDto>()
        );

        var aiJsonResponse = JsonSerializer.Serialize(aiData);

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BudgetItemsAiResult(aiJsonResponse, userRequest, "llama3.2"));

        _dateParsingServiceMock.Setup(x => x.ParseDate("2025-01-15")).Returns(new DateOnly(2025, 1, 15));
        _dateParsingServiceMock.Setup(x => x.ParseDate("invalid date")).Returns((DateOnly?)null);

        _itemMatchingServiceMock
            .Setup(x => x.MatchItemsAsync(_companyId, It.IsAny<List<AiItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchedItemDto>());

        var query = new GenerateInvoiceWithAiQuery(new PrivateInvoiceAiRequestDto(userRequest));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(new DateOnly(2025, 1, 15), result.IssueDate);
        Assert.Null(result.DueDate);
    }

    [Fact]
    public async Task Handle_WithMalformedJson_ThrowsAppException()
    {
        // Arrange
        var userRequest = "Request that returns invalid JSON";

        _aiServiceMock
            .Setup(x => x.GenerateJsonAsync(It.IsAny<string>(), userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BudgetItemsAiResult("{invalid json", userRequest, "llama3.2"));

        var query = new GenerateInvoiceWithAiQuery(new PrivateInvoiceAiRequestDto(userRequest));

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
