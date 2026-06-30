namespace Budgexa.API.Tests.Middleware;

using System.Net;
using System.Text.Json;
using Budgexa.API.Middleware;
using Budgexa.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

public class GlobalExceptionHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static HttpContext CreateContext(out MemoryStream body)
    {
        var context = new DefaultHttpContext();
        body = new MemoryStream();
        context.Response.Body = body;
        return context;
    }

    private static GlobalExceptionHandler CreateHandler(bool isDevelopment)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(isDevelopment ? Environments.Development : Environments.Production);
        return new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, environment);
    }

    private static async Task<ApiErrorResponse> ReadResponseAsync(MemoryStream body)
    {
        body.Position = 0;
        return (await JsonSerializer.DeserializeAsync<ApiErrorResponse>(body, JsonOptions))!;
    }

    [Fact]
    public async Task TryHandleAsync_AppException_WritesStatusCodeAndPayload()
    {
        var context = CreateContext(out var body);
        var handler = CreateHandler(isDevelopment: false);

        var metadata = new Dictionary<string, string> { { "field", "value" } };
        var exception = new AppException(HttpStatusCode.BadRequest, ErrorTags.Validation.Failed, "bad input", metadata);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var payload = await ReadResponseAsync(body);
        payload.Tag.Should().Be(ErrorTags.Validation.Failed);
        payload.Message.Should().Be("bad input");
        payload.Metadata.Should().NotBeNull();
        payload.Metadata!["field"].Should().Be("value");
        payload.Detail.Should().BeNull();
        payload.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_ReturnsInternalServerError()
    {
        var context = CreateContext(out var body);
        var handler = CreateHandler(isDevelopment: false);

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        var payload = await ReadResponseAsync(body);
        payload.Tag.Should().Be(ErrorTags.Server.InternalError);
        payload.Message.Should().Be("An unexpected error occurred.");
        payload.Metadata.Should().BeNull();
        payload.Detail.Should().BeNull();
    }

    [Fact]
    public async Task TryHandleAsync_Development_IncludesExceptionDetail()
    {
        var context = CreateContext(out var body);
        var handler = CreateHandler(isDevelopment: true);

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        handled.Should().BeTrue();

        var payload = await ReadResponseAsync(body);
        payload.Detail.Should().NotBeNullOrWhiteSpace();
        payload.Detail!.Should().Contain("InvalidOperationException");
        payload.Detail.Should().Contain("boom");
    }
}
