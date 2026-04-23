using Budgexa.Domain.Entities;
using Budgexa.Domain.Static;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Budgexa.Infrastructure.Persistence.Configurations;

public class StatusConfiguration : IEntityTypeConfiguration<Status>
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
            Status.Create("Base", "Delete", 2, StatusIds.Delete)
        );
    }
}