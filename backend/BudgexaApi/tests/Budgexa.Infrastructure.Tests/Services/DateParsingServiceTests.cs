namespace Budgexa.Infrastructure.Tests.Services;

using Budgexa.Infrastructure.Services;

public sealed class DateParsingServiceTests
{
    private readonly DateParsingService _service = new();

    [Fact]
    public void ParseDate_WithIsoFormat_ReturnsDate()
    {
        // Arrange
        var dateString = "2025-01-15";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }

    [Fact]
    public void ParseDate_WithSpanishLongFormat_ReturnsDate()
    {
        // Arrange
        var dateString = "15 de enero de 2025";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }

    [Fact]
    public void ParseDate_WithSpanishShortFormat_ReturnsDate()
    {
        // Arrange
        var dateString = "15/01/2025";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }

    [Fact]
    public void ParseDate_WithEnglishLongFormat_ReturnsDate()
    {
        // Arrange
        var dateString = "January 15, 2025";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }

    [Fact]
    public void ParseDate_WithEnglishShortFormat_ReturnsDate()
    {
        // Arrange
        var dateString = "01/15/2025";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }

    [Fact]
    public void ParseDate_WithGermanFormat_ReturnsDate()
    {
        // Arrange
        var dateString = "15.01.2025";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }

    [Fact]
    public void ParseDate_WithInvalidString_ReturnsNull()
    {
        // Arrange
        var dateString = "invalid date string";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseDate_WithNull_ReturnsNull()
    {
        // Act
        var result = _service.ParseDate(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseDate_WithEmptyString_ReturnsNull()
    {
        // Act
        var result = _service.ParseDate(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseDate_WithWhitespace_ReturnsNull()
    {
        // Act
        var result = _service.ParseDate("   ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseDate_WithExtraSpaces_ReturnsDate()
    {
        // Arrange
        var dateString = " 15 de enero de 2025 ";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }

    [Fact]
    public void ParseDate_WithDifferentYear_ReturnsDate()
    {
        // Arrange
        var dateString = "2024-12-31";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 12, 31), result);
    }

    [Fact]
    public void ParseDate_WithDashFormat_ReturnsDate()
    {
        // Arrange
        var dateString = "15-01-2025";

        // Act
        var result = _service.ParseDate(dateString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2025, 1, 15), result);
    }
}
