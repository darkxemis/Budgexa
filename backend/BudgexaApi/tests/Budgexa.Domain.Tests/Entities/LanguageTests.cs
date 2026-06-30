namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class LanguageTests
{
    [Fact]
    public void Create_WithoutExplicitId_GeneratesNewGuid()
    {
        var language = Language.Create("en", "English");

        language.Id.Should().NotBe(Guid.Empty);
        language.Code.Should().Be("en");
        language.Name.Should().Be("English");
        language.Translations.Should().BeEmpty();
        language.StatusTranslations.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var language = Language.Create("es", "Spanish", id);

        language.Id.Should().Be(id);
    }
}
