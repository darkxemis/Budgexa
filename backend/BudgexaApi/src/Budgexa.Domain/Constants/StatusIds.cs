namespace Budgexa.Domain.Constants;

public static class StatusIds
{
    public const string NewString = "a1e2b3c4-d5f6-4a7b-8c9d-0e1f2a3b4c5d";
    public const string DeleteString = "e5d4c3b2-a1f0-4e9d-8c7b-6a5f4e3d2c1b";

    public static readonly Guid New = new(NewString);
    public static readonly Guid Delete = new(DeleteString);

    public static class Budget
    {
        public static readonly Guid Draft = Guid.Parse("11111111-1111-4111-8111-111111111111");
        public static readonly Guid Sent = Guid.Parse("22222222-1111-4111-8111-111111111111");
        public static readonly Guid Approved = Guid.Parse("33333333-1111-4111-8111-111111111111");
        public static readonly Guid Rejected = Guid.Parse("44444444-1111-4111-8111-111111111111");
        public static readonly Guid Expired = Guid.Parse("55555555-1111-4111-8111-111111111111");
        public static readonly Guid Invoiced = Guid.Parse("66666666-1111-4111-8111-111111111111");
    }

    public static class Invoice
    {
        public static readonly Guid Draft = Guid.Parse("11111111-2222-4222-8222-222222222222");
        public static readonly Guid Issued = Guid.Parse("22222222-2222-4222-8222-222222222222");
        public static readonly Guid PartiallyPaid = Guid.Parse("33333333-2222-4222-8222-222222222222");
        public static readonly Guid Paid = Guid.Parse("44444444-2222-4222-8222-222222222222");
        public static readonly Guid Overdue = Guid.Parse("55555555-2222-4222-8222-222222222222");
        public static readonly Guid Cancelled = Guid.Parse("66666666-2222-4222-8222-222222222222");
    }
}