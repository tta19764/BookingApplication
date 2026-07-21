using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApp.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for conference hall persistence.
/// </summary>
public class HallConfiguration : IEntityTypeConfiguration<ConferenceHall>
{
    public void Configure(EntityTypeBuilder<ConferenceHall> builder)
    {
        builder.ToTable("conference_halls");

        builder.HasKey(hall => hall.Id);

        builder.Property(hall => hall.Id)
            .ValueGeneratedNever();

        builder.Property(hall => hall.Name)
            .HasConversion(
                name => name.Value,
                value => new Name(value))
            .HasMaxLength(100)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(hall => hall.Seats)
            .HasConversion(
                capacity => capacity.Value,
                value => new Capacity(value))
            .HasColumnName("capacity")
            .IsRequired();

        builder.OwnsOne(hall => hall.Price, priceBuilder =>
        {
            priceBuilder.Property(price => price.Amount)
                .HasColumnName("hourly_rate")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(price => price.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("currency")
                .IsRequired();
        });

        builder.Property(hall => hall.LastBookedOnUtc)
            .HasColumnName("last_booked_on_utc");

        // Store the fixed amenity enum list in one column because the catalog is small and bounded.
        builder.Property(hall => hall.Amenities)
            .HasConversion(
                amenities => string.Join(",", amenities.Select(amenity => (int)amenity)),
                value => string.IsNullOrWhiteSpace(value)
                    ? new List<Amenity>()
                    : value.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => (Amenity)int.Parse(item))
                        .ToList())
            // EF needs content-based comparison because List<T> is mutable and reference comparison is not enough.
            .Metadata.SetValueComparer(new ValueComparer<List<Amenity>>(
                (first, second) => first != null && second != null && first.SequenceEqual(second),
                amenities => amenities.Aggregate(0, (hash, amenity) => HashCode.Combine(hash, amenity.GetHashCode())),
                amenities => amenities.ToList()));

        builder.Property(hall => hall.Amenities)
            .HasColumnName("amenities")
            .IsRequired();

        builder.HasMany(hall => hall.Bookings)
            .WithOne(booking => booking.ConferenceHall)
            .HasForeignKey(booking => booking.ConferenceHallId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
