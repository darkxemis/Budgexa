namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class PublicBudgetLineConfiguration : IEntityTypeConfiguration<PublicBudgetLine>
{
    public void Configure(EntityTypeBuilder<PublicBudgetLine> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.PublicBudgetId)
            .IsRequired();

        builder.Property(l => l.ItemId)
            .IsRequired();

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Quantity)
            .HasPrecision(18, 4);

        builder.Property(l => l.Price)
            .HasPrecision(18, 4);

        builder.Property(l => l.TaxPercentage)
            .HasPrecision(5, 2);

        builder.Property(l => l.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(l => l.SubTotal)
            .HasPrecision(18, 2);

        builder.HasOne(l => l.Item)
            .WithMany()
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("PublicBudgetLines");
    }
}
