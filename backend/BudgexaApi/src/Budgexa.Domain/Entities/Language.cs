namespace Budgexa.Domain.Entities;

public sealed class Language
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    public ICollection<StatusTranslation> StatusTranslations { get; private set; } = new List<StatusTranslation>();

    private Language() { }

    public static Language Create(string code, string name, Guid? id = null)
        => new Language { Id = id ?? Guid.NewGuid(), Code = code, Name = name };
}