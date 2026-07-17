namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        // Base Entity columns
        builder.Property(c => c.Id).HasColumnOrder(0);
        builder.Property(c => c.CreatedAt).HasColumnOrder(1);
        builder.Property(c => c.UpdatedAt).HasColumnOrder(2);
        builder.Property(c => c.CreatedByUserId).HasColumnOrder(3);
        builder.Property(c => c.UpdatedByUserId).HasColumnOrder(4);

        // Foreign keys
        builder.Property(c => c.CompanyId).IsRequired().HasColumnOrder(5);
        builder.Property(c => c.StatusId).IsRequired().HasColumnOrder(6);

        // Customer properties
        builder.Property(c => c.LegalName).IsRequired().HasMaxLength(200).HasColumnOrder(7);
        builder.Property(c => c.TradeName).HasMaxLength(200).HasColumnOrder(8);
        builder.Property(c => c.TaxId).IsRequired().HasMaxLength(50).HasColumnOrder(9);
        builder.Property(c => c.Email).HasMaxLength(200).HasColumnOrder(10);
        builder.Property(c => c.Phone).HasMaxLength(50).HasColumnOrder(11);

        builder.Property(c => c.AddressLine).HasMaxLength(300).HasColumnOrder(12);
        builder.Property(c => c.City).HasMaxLength(150).HasColumnOrder(13);
        builder.Property(c => c.PostalCode).HasMaxLength(20).HasColumnOrder(14);
        builder.Property(c => c.Province).HasMaxLength(150).HasColumnOrder(15);
        builder.Property(c => c.Country).HasMaxLength(100).HasColumnOrder(16);

        builder.Property(c => c.Notes).HasMaxLength(2000).HasColumnOrder(17);

        // Unique TaxId per Company
        builder.HasIndex(c => new { c.CompanyId, c.TaxId }).IsUnique();

        // Relationships
        builder.HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Status)
            .WithMany()
            .HasForeignKey(c => c.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Budgets)
            .WithOne(b => b.Customer)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.Customer)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Customers");
    }
}
