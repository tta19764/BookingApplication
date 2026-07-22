using BookingApp.Application.ConferenceHalls.AddHall;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.ConferenceHalls;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Application.UnitTests.ConferenceHalls;

public class AddHallCommandHandlerTests
{
    private readonly IConferenceHallRepository _hallRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AddHallCommandHandler _handler;

    public AddHallCommandHandlerTests()
    {
        _hallRepositoryMock = Substitute.For<IConferenceHallRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _handler = new AddHallCommandHandler(_hallRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_Should_AddHallAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = new AddHallCommand(
            "  Board Room  ",
            24,
            1200m,
            "uah",
            [Amenity.Projector, Amenity.Projector, Amenity.WiFi]);

        // Act
        Result<Guid> result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _hallRepositoryMock.Received(1).Add(Arg.Is<ConferenceHall>(hall =>
            hall.Id == result.Value &&
            hall.Name.Value == "Board Room" &&
            hall.Seats.Value == 24 &&
            hall.Price.Amount == 1200m &&
            hall.Price.Currency.Code == "UAH" &&
            hall.Amenities.SequenceEqual(new[] { Amenity.Projector, Amenity.WiFi })));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }
}
