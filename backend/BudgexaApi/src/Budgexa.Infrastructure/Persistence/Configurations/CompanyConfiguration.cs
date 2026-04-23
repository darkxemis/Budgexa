using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Budgexa.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        // Primary key
        builder.HasKey(c => c.Id);

        // Column order for base Entity properties
        builder.Property(c => c.Id).HasColumnOrder(0);
        builder.Property(c => c.CreatedAt).HasColumnOrder(1);
        builder.Property(c => c.UpdatedAt).HasColumnOrder(2);
        builder.Property(c => c.CreatedByUserId).HasColumnOrder(3);
        builder.Property(c => c.UpdatedByUserId).HasColumnOrder(4);

        // Company properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnOrder(5);

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .HasColumnOrder(6);

        builder.Property(c => c.StartDate)
            .IsRequired()
            .HasColumnOrder(7);

        builder.Property(c => c.EndDate)
            .HasColumnOrder(8);

        // One-to-many relationship with User
        builder.HasMany(c => c.Users)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId);

        builder.ToTable("Companies");
    }
}