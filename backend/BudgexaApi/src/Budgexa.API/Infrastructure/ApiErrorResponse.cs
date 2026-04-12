namespace Budgexa.API.Infrastructure;

public sealed record ApiErrorResponse(
    string Tag,
    string Message,
    string TraceId,
    Dictionary<string, string>? Metadata = null,
    string? Detail = null);
