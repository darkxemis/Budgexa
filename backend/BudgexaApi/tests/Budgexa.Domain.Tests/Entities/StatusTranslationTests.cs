namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class StatusTranslationTests
{
    [Fact]
    public void Create_WithoutExplicitId_GeneratesNewGuid()
    {
        var statusId = Guid.NewGuid();
        var languageId = Guid.NewGuid();

        var translation = StatusTranslation.Create(statusId, languageId, "Nuevo");

        translation.Id.Should().NotBe(Guid.Empty);
        translation.StatusId.Should().Be(statusId);
        translation.LanguageId.Should().Be(languageId);
        translation.Translation.Should().Be("Nuevo");
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var translation = StatusTranslation.Create(Guid.NewGuid(), Guid.NewGuid(), "New", id);

        translation.Id.Should().Be(id);
    }
}
