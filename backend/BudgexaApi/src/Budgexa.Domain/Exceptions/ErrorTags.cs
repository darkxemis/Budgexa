namespace Budgexa.Domain.Exceptions;

public static class ErrorTags
{
    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalidCredentials";
        public const string InvalidRefreshToken = "auth.invalidRefreshToken";
        public const string AccountLocked = "auth.accountLocked";
    }

    public static class User
    {
        public const string NotFound = "user.notFound";
        public const string EmailAlreadyExists = "user.emailAlreadyExists";
    }

    public static class Role
    {
        public const string NotFound = "role.notFound";
        public const string NameExists = "role.nameExists";
    }

    public static class Status
    {
        public const string NotFound = "status.notFound";
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
