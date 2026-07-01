namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Group)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired();

        builder.HasMany(s => s.Translations)
            .WithOne(t => t.Status)
            .HasForeignKey(t => t.StatusId);

        builder.ToTable("Statuses");

        builder.HasData(
            Status.Create("Base", "New", 1, StatusIds.New),
            Status.Create("Base", "Delete", 2, StatusIds.Delete),

            // Budget statuses
            Status.Create("Budget", "Draft", 1, StatusIds.Budget.Draft),
            Status.Create("Budget", "Sent", 2, StatusIds.Budget.Sent),
            Status.Create("Budget", "Approved", 3, StatusIds.Budget.Approved),
            Status.Create("Budget", "Rejected", 4, StatusIds.Budget.Rejected),
            Status.Create("Budget", "Expired", 5, StatusIds.Budget.Expired),
            Status.Create("Budget", "Invoiced", 6, StatusIds.Budget.Invoiced),

            // Invoice statuses
            Status.Create("Invoice", "Draft", 1, StatusIds.Invoice.Draft),
            Status.Create("Invoice", "Issued", 2, StatusIds.Invoice.Issued),
            Status.Create("Invoice", "PartiallyPaid", 3, StatusIds.Invoice.PartiallyPaid),
            Status.Create("Invoice", "Paid", 4, StatusIds.Invoice.Paid),
            Status.Create("Invoice", "Overdue", 5, StatusIds.Invoice.Overdue),
            Status.Create("Invoice", "Cancelled", 6, StatusIds.Invoice.Cancelled)
        );
    }
}