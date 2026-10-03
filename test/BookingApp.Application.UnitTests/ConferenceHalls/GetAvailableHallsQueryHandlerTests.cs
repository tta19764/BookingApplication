using BookingApp.Application.ConferenceHalls.GetAvailableHalls;
using BookingApp.Application.ConferenceHalls.GetHall;
using BookingApp.Application.UnitTests.Infrastructure;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Application.UnitTests.ConferenceHalls;

public class GetAvailableHallsQueryHandlerTests
{
    private readonly IConferenceHallRepository _hallRepositoryMock;
    private readonly GetAvailableHallsQueryHandler _handler;

    public GetAvailableHallsQueryHandlerTests()
    {
        _hallRepositoryMock = Substitute.For<IConferenceHallRepository>();
        _handler = new GetAvailableHallsQueryHandler(_hallRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnMappedAvailableHalls()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var query = new GetAvailableHallsQuery(new DateOnly(2026, 7, 23), "10:40", "12:10", 20);
        ConferenceHall hall = HallData.Create(capacity: 50);

        _hallRepositoryMock
            .GetAvailableConferenceHalls(
                Arg.Any<DateRange>(),
                Arg.Is<Capacity>(capacity => capacity.Value == query.Capacity),
                cancellationToken)
            .Returns([hall]);

        // Act
        Result<IEnumerable<HallResponse>> result =
            await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(response =>
            response.Id == hall.Id &&
            response.Name == hall.Name.Value &&
            response.Capacity == hall.Seats.Value);
    }
}
