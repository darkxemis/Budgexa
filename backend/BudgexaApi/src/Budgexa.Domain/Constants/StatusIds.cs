namespace Budgexa.Domain.Constants;

public static class StatusIds
{
    public const string NewString = "a1e2b3c4-d5f6-4a7b-8c9d-0e1f2a3b4c5d";
    public const string DeleteString = "e5d4c3b2-a1f0-4e9d-8c7b-6a5f4e3d2c1b";

    public static readonly Guid New = new(NewString);
    public static readonly Guid Delete = new(DeleteString);
}