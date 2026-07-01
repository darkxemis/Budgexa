namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
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
        builder.Property(i => i.CustomerId).IsRequired().HasColumnOrder(6);
        builder.Property(i => i.StatusId).IsRequired().HasColumnOrder(7);
        builder.Property(i => i.BudgetId).HasColumnOrder(8);

        // Invoice properties
        builder.Property(i => i.Series).IsRequired().HasMaxLength(20).HasColumnOrder(9);
        builder.Property(i => i.Number).IsRequired().HasMaxLength(50).HasColumnOrder(10);
        builder.Property(i => i.IssueDate).IsRequired().HasColumnType("date").HasColumnOrder(11);
        builder.Property(i => i.DueDate).IsRequired().HasColumnType("date").HasColumnOrder(12);

        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3).IsFixedLength().HasColumnOrder(13);
        builder.Property(i => i.Subtotal).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(14);
        builder.Property(i => i.TaxAmount).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(15);
        builder.Property(i => i.WithholdingAmount).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(16);
        builder.Property(i => i.Total).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(17);
        builder.Property(i => i.AmountPaid).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(18);

        builder.Property(i => i.PaymentMethod).HasConversion<int>().HasColumnOrder(19);
        builder.Property(i => i.PaymentReference).HasMaxLength(200).HasColumnOrder(20);
        builder.Property(i => i.Notes).HasMaxLength(2000).HasColumnOrder(21);

        // Unique Series + Number per Company
        builder.HasIndex(i => new { i.CompanyId, i.Series, i.Number }).IsUnique();

        // Relationships
        builder.HasOne(i => i.Company)
            .WithMany()
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Status)
            .WithMany()
            .HasForeignKey(i => i.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Budget)
            .WithMany(b => b.Invoices)
            .HasForeignKey(i => i.BudgetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Lines)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Invoice.Lines))!
            .SetField("_lines");

        builder.Metadata
            .FindNavigation(nameof(Invoice.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("Invoices");
    }
}
