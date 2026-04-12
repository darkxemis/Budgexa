namespace Budgexa.Domain.Exceptions;

public static class ErrorTags
{
    public static class Auth
    {
        public const string EmailAlreadyExists = "auth.emailAlreadyExists";
        public const string InvalidCredentials = "auth.invalidCredentials";
    }

    public static class User
    {
        public const string NotFound = "user.notFound";
    }

    public static class Server
    {
        public const string InternalError = "server.internalError";
    }

    public static class Validation
    {
        public const string Failed = "validation.failed";
    }
}
