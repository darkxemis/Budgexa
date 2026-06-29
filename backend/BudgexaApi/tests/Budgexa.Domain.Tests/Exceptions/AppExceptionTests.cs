namespace Budgexa.Domain.Tests.Exceptions;

using System.Net;
using Budgexa.Domain.Exceptions;

public class AppExceptionTests
{
    [Fact]
    public void Constructor_WithMinimumArguments_StoresProperties()
    {
        var exception = new AppException(HttpStatusCode.NotFound, ErrorTags.User.NotFound, "User not found.");

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Tag.Should().Be(ErrorTags.User.NotFound);
        exception.Message.Should().Be("User not found.");
        exception.Metadata.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMetadata_StoresMetadata()
    {
        var metadata = new Dictionary<string, string>
        {
            { "field", "value" }
        };

        var exception = new AppException(HttpStatusCode.BadRequest, ErrorTags.Validation.Failed, "Validation failed.", metadata);

        exception.Metadata.Should().BeSameAs(metadata);
    }

    [Fact]
    public void AppException_IsAnException()
    {
        var exception = new AppException(HttpStatusCode.Conflict, ErrorTags.User.EmailAlreadyExists, "Email exists.");

        exception.Should().BeAssignableTo<Exception>();
    }
}
