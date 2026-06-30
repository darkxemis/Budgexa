namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
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