using BookingApp.Bll.Common.Users.Models;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApp.Dal.SqlRepositories.Configurations;

/// <summary>
/// EF Core mapping for application users and their role assignments.
/// </summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.FirstName)
            .HasConversion(
                firstName => firstName.Value,
                value => new FirstName(value))
            .HasMaxLength(100)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasConversion(
                lastName => lastName.Value,
                value => new LastName(value))
            .HasMaxLength(100)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(user => user.Email)
            .HasConversion(
                email => email.Value,
                value => new Email(value))
            .HasMaxLength(320)
            .HasColumnName("email")
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        // Roles are exposed as a read-only collection backed by a private field on the aggregate.
        builder.HasMany(user => user.Roles)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "user_roles",
                rightBuilder => rightBuilder
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("role_id")
                    .OnDelete(DeleteBehavior.Cascade),
                leftBuilder => leftBuilder
                    .HasOne<UserEntity>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .OnDelete(DeleteBehavior.Cascade),
                joinBuilder =>
                {
                    joinBuilder.ToTable("user_roles");
                    joinBuilder.HasKey("user_id", "role_id");
                });

        builder.Navigation(user => user.Roles)
            // Force EF to use the backing field instead of the defensive-copy Roles property.
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}
