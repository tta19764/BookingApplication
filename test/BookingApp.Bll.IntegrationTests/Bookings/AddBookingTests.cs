using BookingApp.Bll.Common.Models;
using BookingApp.Services.Web.Extensions;
using BookingApp.Bll.Bookings.AddBooking;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Bll.IntegrationTests.Bookings;

public class AddBookingTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task AddBooking_Should_PersistBookingAndReturnPriceBreakdown()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        ConferenceHall hall = await DbContext
            .Set<ConferenceHall>()
            .AsNoTracking()
            .FirstAsync(cancellationToken);

        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var command = new AddBookingRequest(
            hall.Id,
            SeedDataExtensions.SeededUserId,
            date,
            "10:40",
            "12:10",
            [Amenity.Projector]);

        // Act
        Result<BookingConfirmationModel> result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HallId.Should().Be(hall.Id);
        result.Value.Currency.Should().Be("UAH");
        result.Value.TotalPrice.Should().BeGreaterThan(result.Value.PriceForPeriod);

        var booking = await DbContext
            .Set<Booking>()
            .AsNoTracking()
            .FirstOrDefaultAsync(storedBooking => storedBooking.Id == result.Value.BookingId, cancellationToken);

        booking.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.Duration.Start.Kind.Should().Be(DateTimeKind.Utc);
        booking.Duration.End.Kind.Should().Be(DateTimeKind.Utc);
    }
}
