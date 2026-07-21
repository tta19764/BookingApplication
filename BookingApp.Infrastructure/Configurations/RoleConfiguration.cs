using BookingApp.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApp.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for roles and their permission assignments.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .ValueGeneratedNever();

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .HasColumnName("name")
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.HasMany(role => role.Permissions)
            .WithMany()
            .UsingEntity<RolePermission>();

        // Seed the baseline role used by the current single-user execution model.
        builder.HasData(Role.Registered);
    }
}
