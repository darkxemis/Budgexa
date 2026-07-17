namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.Id);

        // Base Entity columns
        builder.Property(i => i.Id).HasColumnOrder(0);
        builder.Property(i => i.CreatedAt).HasColumnOrder(1);
        builder.Property(i => i.UpdatedAt).HasColumnOrder(2);
        builder.Property(i => i.CreatedByUserId).HasColumnOrder(3);
        builder.Property(i => i.UpdatedByUserId).HasColumnOrder(4);

        // Foreign keys
        builder.Property(i => i.CompanyId).IsRequired().HasColumnOrder(5);
        builder.Property(i => i.StatusId).IsRequired().HasColumnOrder(6);

        // Item properties
        builder.Property(i => i.Sku).HasMaxLength(50).HasColumnOrder(7);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200).HasColumnOrder(8);
        builder.Property(i => i.Description).HasMaxLength(1000).HasColumnOrder(9);
        builder.Property(i => i.Type).IsRequired().HasConversion<int>().HasColumnOrder(10);
        builder.Property(i => i.Unit).IsRequired().HasMaxLength(30).HasColumnOrder(11);
        builder.Property(i => i.UnitPrice).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(12);
        builder.Property(i => i.TaxRate).IsRequired().HasColumnType("decimal(5,2)").HasColumnOrder(13);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3).IsFixedLength().HasColumnOrder(14);

        // Unique SKU per Company (only when SKU is set)
        builder.HasIndex(i => new { i.CompanyId, i.Sku })
            .IsUnique()
            .HasFilter("[Sku] IS NOT NULL");

        // Relationships
        builder.HasOne(i => i.Company)
            .WithMany()
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Status)
            .WithMany()
            .HasForeignKey(i => i.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Items");
    }
}
