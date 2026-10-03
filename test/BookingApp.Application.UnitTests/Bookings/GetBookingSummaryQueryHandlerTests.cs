using BookingApp.Application.Reports.GetBookingSummary;
using BookingApp.Application.UnitTests.Infrastructure;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Application.UnitTests.Bookings;

public class GetBookingSummaryQueryHandlerTests
{
    private readonly IBookingRepository _bookingRepositoryMock;
    private readonly GetBookingSummaryQueryHandler _handler;

    public GetBookingSummaryQueryHandlerTests()
    {
        _bookingRepositoryMock = Substitute.For<IBookingRepository>();
        _handler = new GetBookingSummaryQueryHandler(_bookingRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_AggregateBookingsFromRepositoryPages()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        ConferenceHall firstHall = HallData.Create(hourlyRate: 2000m);
        ConferenceHall secondHall = HallData.Create(hourlyRate: 1500m);

        Booking firstBooking = CreateBooking(firstHall, "10:00", "11:00");
        Booking secondBooking = CreateBooking(firstHall, "12:00", "13:00");
        Booking thirdBooking = CreateBooking(secondHall, "14:00", "15:00");

        _bookingRepositoryMock
            .List(Arg.Any<int>(), cancellationToken)
            .Returns(ToAsyncPages(
                [firstBooking, secondBooking],
                [thirdBooking]));

        // Act
        Result<BookingSummaryResponse> result = await _handler.Handle(new GetBookingSummaryQuery(), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalBookings.Should().Be(3);
        result.Value.TotalRevenue.Should().Be(firstBooking.TotalPrice.Amount + secondBooking.TotalPrice.Amount + thirdBooking.TotalPrice.Amount);
        result.Value.Halls.Should().HaveCount(2);
        result.Value.Halls.Should().Contain(summary => summary.HallId == firstHall.Id && summary.BookingCount == 2);
        result.Value.Halls.Should().Contain(summary => summary.HallId == secondHall.Id && summary.BookingCount == 1);
    }

    private static Booking CreateBooking(ConferenceHall hall, string startTime, string endTime)
    {
        var duration = DateRange.Create(
            new DateOnly(2026, 7, 23),
            TimeOnly.Parse(startTime),
            TimeOnly.Parse(endTime));

        return Booking.Reserve(
            hall,
            [Amenity.Projector],
            Guid.NewGuid(),
            duration,
            new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc),
            new PricingService());
    }

    private static async IAsyncEnumerable<IReadOnlyCollection<Booking>> ToAsyncPages(
        params IReadOnlyCollection<Booking>[] pages)
    {
        foreach (IReadOnlyCollection<Booking> page in pages)
        {
            yield return page;
            await Task.Yield();
        }
    }
}
