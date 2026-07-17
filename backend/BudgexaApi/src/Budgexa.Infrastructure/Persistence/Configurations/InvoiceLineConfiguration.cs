namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnOrder(0);
        builder.Property(l => l.InvoiceId).IsRequired().HasColumnOrder(1);
        builder.Property(l => l.ItemId).HasColumnOrder(2);
        builder.Property(l => l.SortOrder).IsRequired().HasColumnOrder(3);

        builder.Property(l => l.Description).IsRequired().HasMaxLength(500).HasColumnOrder(4);
        builder.Property(l => l.Unit).IsRequired().HasMaxLength(30).HasColumnOrder(5);
        builder.Property(l => l.Quantity).IsRequired().HasColumnType("decimal(18,4)").HasColumnOrder(6);
        builder.Property(l => l.UnitPrice).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(7);
        builder.Property(l => l.DiscountPercentage).IsRequired().HasColumnType("decimal(5,2)").HasColumnOrder(8);
        builder.Property(l => l.TaxRate).IsRequired().HasColumnType("decimal(5,2)").HasColumnOrder(9);
        builder.Property(l => l.WithholdingRate).IsRequired().HasColumnType("decimal(5,2)").HasColumnOrder(10);

        builder.Property(l => l.Subtotal).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(11);
        builder.Property(l => l.TaxAmount).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(12);
        builder.Property(l => l.WithholdingAmount).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(13);
        builder.Property(l => l.Total).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(14);

        builder.HasIndex(l => l.InvoiceId);

        builder.HasOne(l => l.Item)
            .WithMany()
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("InvoiceLines");
    }
}
