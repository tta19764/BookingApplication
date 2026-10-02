using BookingApp.Bll.Common.Models;
using BookingApp.Bll.ConferenceHalls.GetAvailableHalls;
using BookingApp.Bll.ConferenceHalls.GetHall;
using BookingApp.Bll.UnitTests.Infrastructure;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Bll.UnitTests.ConferenceHalls;

public class GetAvailableHallsManagerTests
{
    private readonly IConferenceHallRepository _hallRepositoryMock;
    private readonly GetAvailableHallsManager _handler;

    public GetAvailableHallsManagerTests()
    {
        _hallRepositoryMock = Substitute.For<IConferenceHallRepository>();
        _handler = new GetAvailableHallsManager(_hallRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnMappedAvailableHalls()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var query = new GetAvailableHallsRequest(new DateOnly(2026, 7, 23), "10:40", "12:10", 20);
        ConferenceHall hall = HallData.Create(capacity: 50);

        _hallRepositoryMock
            .GetAvailableConferenceHalls(
                Arg.Any<DateRange>(),
                Arg.Is<Capacity>(capacity => capacity.Value == query.Capacity),
                cancellationToken)
            .Returns([hall]);

        // Act
        Result<IEnumerable<HallModel>> result =
            await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(response =>
            response.Id == hall.Id &&
            response.Name == hall.Name.Value &&
            response.Capacity == hall.Seats.Value);
    }
}
