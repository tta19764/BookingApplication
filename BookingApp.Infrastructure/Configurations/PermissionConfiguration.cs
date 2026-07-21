using BookingApp.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApp.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for permission catalog entries.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .ValueGeneratedNever();

        builder.Property(permission => permission.Name)
            .HasMaxLength(100)
            .HasColumnName("name")
            .IsRequired();

        builder.HasIndex(permission => permission.Name)
            .IsUnique();

        // Seed the fixed permission catalog used by role-permission mapping.
        builder.HasData(
            Permission.ConferenceHallRead,
            Permission.ConferenceHallWrite,
            Permission.BookingRead,
            Permission.BookingWrite);
    }
}
