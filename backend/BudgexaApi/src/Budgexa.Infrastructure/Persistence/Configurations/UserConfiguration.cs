using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Budgexa.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary key and base Entity properties
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnOrder(0);
        builder.Property(u => u.CreatedAt).HasColumnOrder(1);
        builder.Property(u => u.UpdatedAt).HasColumnOrder(2);
        builder.Property(u => u.CreatedByUserId).HasColumnOrder(3);
        builder.Property(u => u.UpdatedByUserId).HasColumnOrder(4);

        // Foreign keys (relations)
        builder.Property(u => u.CompanyId).IsRequired().HasColumnOrder(5);
        builder.Property(u => u.LanguageId).IsRequired().HasColumnOrder(6);
        builder.Property(u => u.StatusId).IsRequired().HasColumnOrder(7);

        // User properties
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200).HasColumnOrder(8);
        builder.Property(u => u.PasswordHash).IsRequired().HasColumnOrder(9);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100).HasColumnOrder(10);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100).HasColumnOrder(11);
        builder.Property(u => u.FailedLoginAttempts).IsRequired().HasColumnOrder(12);
        builder.Property(u => u.LockoutEnd).HasColumnOrder(13);

        // Relationships
        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId);

        builder.HasOne(u => u.Language)
            .WithMany()
            .HasForeignKey(u => u.LanguageId);

        builder.HasOne(u => u.Status)
            .WithMany()
            .HasForeignKey(u => u.StatusId);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder.ToTable("Users");
    }
}