using Budgexa.Domain.Entities;
using Budgexa.Domain.Static;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Budgexa.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.ToTable("Languages");

        builder.HasData(
            Language.Create("en", "English", LanguageIds.English),
            Language.Create("es", "Spanish", LanguageIds.Spanish),
            Language.Create("de", "German", LanguageIds.German),
            Language.Create("hr", "Croatian", LanguageIds.Croatian)
        );
    }
}