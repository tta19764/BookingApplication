using BookingApp.Api.Extensions;
using BookingApp.Application.Bookings.AddBooking;
using BookingApp.Application.IntegrationTests.Infrastructure;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Application.IntegrationTests.Bookings;

public class AddBookingTests : BaseIntegrationTest
{
    public AddBookingTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

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

        var command = new AddBookingCommand(
            hall.Id,
            SeedDataExtensions.SeededUserId,
            date,
            "10:40",
            "12:10",
            [Amenity.Projector]);

        // Act
        Result<BookingConfirmationResponse> result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HallId.Should().Be(hall.Id);
        result.Value.Currency.Should().Be("UAH");
        result.Value.TotalPrice.Should().BeGreaterThan(result.Value.PriceForPeriod);

        Booking? booking = await DbContext
            .Set<Booking>()
            .AsNoTracking()
            .FirstOrDefaultAsync(storedBooking => storedBooking.Id == result.Value.BookingId, cancellationToken);

        booking.Should().NotBeNull();
        booking!.Status.Should().Be(BookingStatus.Reserved);
        booking.Duration.Start.Kind.Should().Be(DateTimeKind.Utc);
        booking.Duration.End.Kind.Should().Be(DateTimeKind.Utc);
    }
}
