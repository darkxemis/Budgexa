namespace Budgexa.Domain.Entities;

public sealed class LanguageTranslation
{
    public Guid Id { get; private set; }
    public Guid LanguageId { get; private set; }
    public Guid TranslationLanguageId { get; private set; }
    public string Translation { get; private set; } = default!;

    public Language Language { get; private set; } = default!;
    public Language TranslationLanguage { get; private set; } = default!;

    private LanguageTranslation() { }

    public static LanguageTranslation Create(Guid languageId, Guid translationLanguageId, string translation, Guid? id = null)
        => new LanguageTranslation { Id = id ?? Guid.NewGuid(), LanguageId = languageId, TranslationLanguageId = translationLanguageId, Translation = translation };
}