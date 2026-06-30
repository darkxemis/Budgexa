namespace Budgexa.Domain.Entities;

public sealed class Status
{
    public Guid Id { get; private set; }
    public string Group { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public int Value { get; private set; }

    public ICollection<StatusTranslation> Translations { get; private set; } = new List<StatusTranslation>();

    private Status() { }

    public static Status Create(string group, string name, int value, Guid? id = null)
        => new Status { Id = id ?? Guid.NewGuid(), Group = group, Name = name, Value = value };
}