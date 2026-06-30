namespace Budgexa.Domain.Exceptions;

using System.Net;

public class AppException(
    HttpStatusCode statusCode,
    string tag,
    string message,
    Dictionary<string, string>? metadata = null) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string Tag { get; } = tag;

    public Dictionary<string, string>? Metadata { get; } = metadata;
}
