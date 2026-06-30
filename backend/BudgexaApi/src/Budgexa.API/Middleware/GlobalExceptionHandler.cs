namespace Budgexa.API.Middleware;

using System.Diagnostics;
using System.Net;
using Budgexa.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, tag, message, metadata) = exception switch
        {
            AppException app => (app.StatusCode, app.Tag, app.Message, app.Metadata),
            _ => (HttpStatusCode.InternalServerError,
                  ErrorTags.Server.InternalError,
                  "An unexpected error occurred.",
                  (Dictionary<string, string>?)null),
        };

        logger.LogError(exception, "Unhandled exception — Tag: {Tag}, TraceId: {TraceId}", tag, traceId);

        httpContext.Response.StatusCode = (int)statusCode;

        var detail = environment.IsDevelopment()
            ? $"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}"
            : null;

        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(tag, message, traceId, metadata, detail),
            cancellationToken);

        return true;
    }
}
