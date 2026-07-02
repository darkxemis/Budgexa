namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class PublicBudgetConfiguration : IEntityTypeConfiguration<PublicBudget>
{
    public void Configure(EntityTypeBuilder<PublicBudget> builder)
    {
        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.Id).HasColumnOrder(0);
        builder.Property(pb => pb.CreatedAt).HasColumnOrder(1);
        builder.Property(pb => pb.UpdatedAt).HasColumnOrder(2);
        builder.Property(pb => pb.CreatedByUserId).HasColumnOrder(3);
        builder.Property(pb => pb.UpdatedByUserId).HasColumnOrder(4);

        builder.Property(pb => pb.CompanyId)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(pb => pb.LanguageId)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(pb => pb.CustomerFirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnOrder(7);

        builder.Property(pb => pb.CustomerLastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnOrder(8);

        builder.Property(pb => pb.SubTotal)
            .HasPrecision(18, 2)
            .HasColumnOrder(9);

        builder.Property(pb => pb.TaxTotal)
            .HasPrecision(18, 2)
            .HasColumnOrder(10);

        builder.Property(pb => pb.GrandTotal)
            .HasPrecision(18, 2)
            .HasColumnOrder(11);

        builder.HasOne(pb => pb.Company)
            .WithMany()
            .HasForeignKey(pb => pb.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pb => pb.Language)
            .WithMany()
            .HasForeignKey(pb => pb.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pb => pb.Lines)
            .WithOne(l => l.PublicBudget)
            .HasForeignKey(l => l.PublicBudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(pb => pb.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("PublicBudgets");
    }
}
