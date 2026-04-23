namespace Budgexa.Domain.Entities;

public sealed class StatusTranslation
{
    public Guid Id { get; private set; }
    public Guid StatusId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string Translation { get; private set; } = default!;

    public Status Status { get; private set; } = default!;
    public Language Language { get; private set; } = default!;

    private StatusTranslation() { }

    public static StatusTranslation Create(Guid statusId, Guid languageId, string translation, Guid? id = null)
        => new StatusTranslation { Id = id ?? Guid.NewGuid(), StatusId = statusId, LanguageId = languageId, Translation = translation };
}