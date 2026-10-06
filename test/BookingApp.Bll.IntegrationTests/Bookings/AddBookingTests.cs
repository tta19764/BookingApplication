using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Services.Web.Services;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using FluentAssertions;

namespace BookingApp.Bll.IntegrationTests.Bookings;

[Collection("SqlServer")]
public class AddBookingTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task AddBooking_Should_PersistBookingAndReturnPriceBreakdown()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var hall = (await Halls.GetListPaginatedAsync(1, 100, cancellationToken)).First();

        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var model = new CreateBookingModel(hall.Id, SeedDataExtensions.SeededUserId, date,
            new TimeOnly(10, 40), new TimeOnly(12, 10), [Amenity.Projector]);
        Result<BookingConfirmationModel> result = await BookingManager.AddBookingAsync(model, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HallId.Should().Be(hall.Id);
        result.Value.Currency.Should().Be("UAH");
        result.Value.TotalPrice.Should().BeGreaterThan(result.Value.PriceForPeriod);

        var booking = await Bookings.GetByIdAsync(result.Value.BookingId, cancellationToken);

        booking.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.Duration.Start.Kind.Should().Be(DateTimeKind.Utc);
        booking.Duration.End.Kind.Should().Be(DateTimeKind.Utc);
    }
}
