namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class LanguageTranslationTests
{
    [Fact]
    public void Create_WithoutExplicitId_GeneratesNewGuid()
    {
        var languageId = Guid.NewGuid();
        var translationLanguageId = Guid.NewGuid();

        var translation = LanguageTranslation.Create(languageId, translationLanguageId, "Inglés");

        translation.Id.Should().NotBe(Guid.Empty);
        translation.LanguageId.Should().Be(languageId);
        translation.TranslationLanguageId.Should().Be(translationLanguageId);
        translation.Translation.Should().Be("Inglés");
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var translation = LanguageTranslation.Create(Guid.NewGuid(), Guid.NewGuid(), "English", id);

        translation.Id.Should().Be(id);
    }
}
