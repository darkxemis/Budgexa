using Budgexa.Domain.Entities;
using Budgexa.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasData(
            Role.Create("freelance", RoleIds.Freelance),
            Role.Create("administrator", RoleIds.Administrator),
            Role.Create("superadministrator", RoleIds.SuperAdministrator)
        );
    }
}
