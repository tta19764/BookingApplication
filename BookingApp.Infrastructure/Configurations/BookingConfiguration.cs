using BookingApp.Domain.Bookings;
using BookingApp.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApp.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for booking persistence.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.Id)
            .ValueGeneratedNever();

        builder.Property(booking => booking.ConferenceHallId)
            .HasColumnName("conference_hall_id")
            .IsRequired();

        builder.Property(booking => booking.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // DateRange is a value object stored inline on the bookings table.
        builder.OwnsOne(booking => booking.Duration, durationBuilder =>
        {
            durationBuilder.Property(duration => duration.Start)
                .HasColumnName("start")
                .IsRequired();

            durationBuilder.Property(duration => duration.End)
                .HasColumnName("end")
                .IsRequired();
        });

        ConfigureMoney(builder, booking => booking.PriceForPeriod, "price_for_period");
        ConfigureMoney(builder, booking => booking.AmenitiesUpCharge, "amenities_up_charge");
        ConfigureMoney(builder, booking => booking.TotalPrice, "total_price");

        builder.Property(booking => booking.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(booking => booking.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();

        builder.Property(booking => booking.RejectedOnUtc)
            .HasColumnName("rejected_on_utc");

        builder.Property(booking => booking.CompletedOnUtc)
            .HasColumnName("completed_on_utc");

        builder.Property(booking => booking.CancelledOnUtc)
            .HasColumnName("cancelled_on_utc");

        builder.HasOne(booking => booking.ConferenceHall)
            .WithMany(hall => hall.Bookings)
            .HasForeignKey(booking => booking.ConferenceHallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(booking => booking.User)
            .WithMany(user => user.Bookings)
            .HasForeignKey(booking => booking.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureMoney(
        EntityTypeBuilder<Booking> builder,
        System.Linq.Expressions.Expression<Func<Booking, Money?>> navigationExpression,
        string columnPrefix)
    {
        // Money values are stored as amount plus currency columns to preserve the domain value object.
        builder.OwnsOne(navigationExpression, moneyBuilder =>
        {
            moneyBuilder.Property(money => money.Amount)
                .HasColumnName($"{columnPrefix}_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            moneyBuilder.Property(money => money.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName($"{columnPrefix}_currency")
                .IsRequired();
        });
    }
}
