namespace Budgexa.Domain.Tests.Constants;

using Budgexa.Domain.Constants;

public class LanguageIdsTests
{
    [Fact]
    public void LanguageGuids_AreStableAndUnique()
    {
        var ids = new[]
        {
            LanguageIds.English,
            LanguageIds.Spanish,
            LanguageIds.German,
            LanguageIds.Croatian
        };

        ids.Should().OnlyHaveUniqueItems();
        ids.Should().NotContain(Guid.Empty);
    }

    [Fact]
    public void LanguageCodes_AreLowercaseTwoLetters()
    {
        LanguageIds.EnglishCode.Should().Be("en");
        LanguageIds.SpanishCode.Should().Be("es");
        LanguageIds.GermanCode.Should().Be("de");
        LanguageIds.CroatianCode.Should().Be("hr");
    }

    [Fact]
    public void EnglishGuid_MatchesKnownValue()
    {
        LanguageIds.English.Should().Be(Guid.Parse("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"));
    }
}
