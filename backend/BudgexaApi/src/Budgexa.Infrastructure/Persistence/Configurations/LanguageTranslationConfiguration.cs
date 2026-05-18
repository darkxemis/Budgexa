namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class LanguageTranslationConfiguration : IEntityTypeConfiguration<LanguageTranslation>
{
    public void Configure(EntityTypeBuilder<LanguageTranslation> builder)
    {
        builder.HasKey(lt => lt.Id);

        builder.Property(lt => lt.Translation)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(lt => lt.Language)
            .WithMany(l => l.Translations)
            .HasForeignKey(lt => lt.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lt => lt.TranslationLanguage)
            .WithMany()
            .HasForeignKey(lt => lt.TranslationLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("LanguageTranslations");

        builder.HasData(
            // English translations
            LanguageTranslation.Create(LanguageIds.English, LanguageIds.English, "English", Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d")),
            LanguageTranslation.Create(LanguageIds.English, LanguageIds.Spanish, "Inglés", Guid.Parse("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e")),
            LanguageTranslation.Create(LanguageIds.English, LanguageIds.German, "Englisch", Guid.Parse("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f")),
            LanguageTranslation.Create(LanguageIds.English, LanguageIds.Croatian, "Engleski", Guid.Parse("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a")),

            // Spanish translations
            LanguageTranslation.Create(LanguageIds.Spanish, LanguageIds.English, "Spanish", Guid.Parse("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b")),
            LanguageTranslation.Create(LanguageIds.Spanish, LanguageIds.Spanish, "Español", Guid.Parse("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c")),
            LanguageTranslation.Create(LanguageIds.Spanish, LanguageIds.German, "Spanisch", Guid.Parse("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d")),
            LanguageTranslation.Create(LanguageIds.Spanish, LanguageIds.Croatian, "Španjolski", Guid.Parse("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e")),

            // German translations
            LanguageTranslation.Create(LanguageIds.German, LanguageIds.English, "German", Guid.Parse("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f")),
            LanguageTranslation.Create(LanguageIds.German, LanguageIds.Spanish, "Alemán", Guid.Parse("d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a")),
            LanguageTranslation.Create(LanguageIds.German, LanguageIds.German, "Deutsch", Guid.Parse("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b")),
            LanguageTranslation.Create(LanguageIds.German, LanguageIds.Croatian, "Njemački", Guid.Parse("f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c")),

            // Croatian translations
            LanguageTranslation.Create(LanguageIds.Croatian, LanguageIds.English, "Croatian", Guid.Parse("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d")),
            LanguageTranslation.Create(LanguageIds.Croatian, LanguageIds.Spanish, "Croata", Guid.Parse("b4c5d6e7-f8a9-4b0c-1d2e-3f4a5b6c7d8e")),
            LanguageTranslation.Create(LanguageIds.Croatian, LanguageIds.German, "Kroatisch", Guid.Parse("c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f")),
            LanguageTranslation.Create(LanguageIds.Croatian, LanguageIds.Croatian, "Hrvatski", Guid.Parse("d6e7f8a9-b0c1-4d2e-3f4a-5b6c7d8e9f0a"))
        );
    }
}