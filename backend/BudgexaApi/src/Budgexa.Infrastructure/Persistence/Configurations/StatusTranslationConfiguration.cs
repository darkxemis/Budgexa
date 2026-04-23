using Budgexa.Domain.Entities;
using Budgexa.Domain.Static;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Budgexa.Infrastructure.Persistence.Configurations;

public class StatusTranslationConfiguration : IEntityTypeConfiguration<StatusTranslation>
{
    public void Configure(EntityTypeBuilder<StatusTranslation> builder)
    {
        builder.HasKey(st => st.Id);

        builder.Property(st => st.Translation)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(st => st.Status)
            .WithMany(s => s.Translations)
            .HasForeignKey(st => st.StatusId);

        builder.HasOne(st => st.Language)
            .WithMany(l => l.StatusTranslations)
            .HasForeignKey(st => st.LanguageId);

        builder.ToTable("StatusTranslations");

        builder.HasData(
            // Status "New"
            StatusTranslation.Create(StatusIds.New, LanguageIds.English, "New", Guid.Parse("b1e2c3d4-5f6a-4b7c-8d9e-0f1a2b3c4d5e")),
            StatusTranslation.Create(StatusIds.New, LanguageIds.Spanish, "Nuevo", Guid.Parse("c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f")),
            StatusTranslation.Create(StatusIds.New, LanguageIds.German, "Neu", Guid.Parse("d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a")),
            StatusTranslation.Create(StatusIds.New, LanguageIds.Croatian, "Novi", Guid.Parse("e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b")),
            // Status "Delete"
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.English, "Delete", Guid.Parse("f5a6b7c8-d9e0-4f1a-2b3c-4d5e6f7a8b9c")),
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.Spanish, "Eliminar", Guid.Parse("a6b7c8d9-e0f1-4a2b-3c4d-5e6f7a8b9c0d")),
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.German, "Löschen", Guid.Parse("b7c8d9e0-f1a2-4b3c-4d5e-6f7a8b9c0d1e")),
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.Croatian, "Izbriši", Guid.Parse("c8d9e0f1-a2b3-4c4d-5e6f-7a8b9c0d1e2f"))
        );
    }
}