namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.ToTable("roles");

        builder.HasData(
            Role.Create("freelance", RoleIds.Freelance),
            Role.Create("administrator", RoleIds.Administrator),
            Role.Create("superadministrator", RoleIds.SuperAdministrator)
        );
    }
}
