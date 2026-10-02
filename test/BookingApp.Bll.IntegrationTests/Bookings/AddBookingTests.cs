using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Services.Web.Services;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Dal.SqlRepositories.Entities;
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
        ConferenceHallEntity hall = await DbContext
            .Set<ConferenceHallEntity>()
            .AsNoTracking()
            .FirstAsync(cancellationToken);

        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Result<BookingConfirmationModel> result = await BookingManager.AddBookingAsync(hall.Id,
            SeedDataExtensions.SeededUserId, date, "10:40", "12:10", [Amenity.Projector], cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HallId.Should().Be(hall.Id);
        result.Value.Currency.Should().Be("UAH");
        result.Value.TotalPrice.Should().BeGreaterThan(result.Value.PriceForPeriod);

        var booking = await DbContext
            .Set<BookingEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(storedBooking => storedBooking.Id == result.Value.BookingId, cancellationToken);

        booking.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.Duration.Start.Kind.Should().Be(DateTimeKind.Utc);
        booking.Duration.End.Kind.Should().Be(DateTimeKind.Utc);
    }
}
