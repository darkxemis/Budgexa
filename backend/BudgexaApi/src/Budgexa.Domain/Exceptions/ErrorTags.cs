namespace Budgexa.Domain.Exceptions;

public static class ErrorTags
{
    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalidCredentials";
        public const string InvalidRefreshToken = "auth.invalidRefreshToken";
        public const string AccountLocked = "auth.accountLocked";
        public const string ContractExpired = "auth.contractExpired";
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

    public static class Customer
    {
        public const string NotFound = "customer.notFound";
        public const string TaxIdAlreadyExists = "customer.taxIdAlreadyExists";
    }

    public static class Item
    {
        public const string NotFound = "item.notFound";
        public const string SkuAlreadyExists = "item.skuAlreadyExists";
    }

    public static class Budget
    {
        public const string NotFound = "budget.notFound";
        public const string NumberAlreadyExists = "budget.numberAlreadyExists";
        public const string InvalidStatusTransition = "budget.invalidStatusTransition";
        public const string LineNotFound = "budget.lineNotFound";
    }

    public static class Invoice
    {
        public const string NotFound = "invoice.notFound";
        public const string NumberAlreadyExists = "invoice.numberAlreadyExists";
        public const string InvalidStatusTransition = "invoice.invalidStatusTransition";
        public const string LineNotFound = "invoice.lineNotFound";
        public const string AlreadyPaid = "invoice.alreadyPaid";
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
