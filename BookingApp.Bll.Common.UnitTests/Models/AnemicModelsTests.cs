using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Shared;
using FluentAssertions;
using Xunit;

namespace BookingApp.Bll.Common.UnitTests.Models;

public sealed class AnemicModelsTests
{
    [Fact]
    public void Booking_Should_StoreAssignedData()
    {
        // Arrange
        var duration = new DateRange
        {
            Start = new DateTime(2026, 10, 3, 10, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2026, 10, 3, 11, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var booking = new Booking(Guid.NewGuid())
        {
            Duration = duration,
            Status = BookingStatus.Reserved,
            TotalPrice = new Money(2000m, Currency.Uah)
        };

        // Assert
        booking.Duration.Should().Be(duration);
        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.TotalPrice.Amount.Should().Be(2000m);
    }
}
