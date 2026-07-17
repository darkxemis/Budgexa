namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);

        // Base Entity columns
        builder.Property(b => b.Id).HasColumnOrder(0);
        builder.Property(b => b.CreatedAt).HasColumnOrder(1);
        builder.Property(b => b.UpdatedAt).HasColumnOrder(2);
        builder.Property(b => b.CreatedByUserId).HasColumnOrder(3);
        builder.Property(b => b.UpdatedByUserId).HasColumnOrder(4);

        // Foreign keys
        builder.Property(b => b.CompanyId).IsRequired().HasColumnOrder(5);
        builder.Property(b => b.CustomerId).IsRequired().HasColumnOrder(6);
        builder.Property(b => b.StatusId).IsRequired().HasColumnOrder(7);

        // Budget properties
        builder.Property(b => b.Number).IsRequired().HasMaxLength(50).HasColumnOrder(8);
        builder.Property(b => b.IssueDate).IsRequired().HasColumnType("date").HasColumnOrder(9);
        builder.Property(b => b.ValidUntil).HasColumnType("date").HasColumnOrder(10);

        builder.Property(b => b.Currency).IsRequired().HasMaxLength(3).IsFixedLength().HasColumnOrder(11);
        builder.Property(b => b.Subtotal).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(12);
        builder.Property(b => b.TaxAmount).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(13);
        builder.Property(b => b.Total).IsRequired().HasColumnType("decimal(18,2)").HasColumnOrder(14);

        builder.Property(b => b.Notes).HasMaxLength(2000).HasColumnOrder(15);
        builder.Property(b => b.TermsAndConditions).HasMaxLength(4000).HasColumnOrder(16);

        // Unique Number per Company
        builder.HasIndex(b => new { b.CompanyId, b.Number }).IsUnique();

        // Relationships
        builder.HasOne(b => b.Company)
            .WithMany()
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Customer)
            .WithMany(c => c.Budgets)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Status)
            .WithMany()
            .HasForeignKey(b => b.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lines as backing field collection
        builder.HasMany(b => b.Lines)
            .WithOne(l => l.Budget)
            .HasForeignKey(l => l.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Budget.Lines))!
            .SetField("_lines");

        builder.Metadata
            .FindNavigation(nameof(Budget.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("Budgets");
    }
}
