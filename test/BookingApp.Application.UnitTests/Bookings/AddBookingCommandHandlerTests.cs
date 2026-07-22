using BookingApp.Application.Abstractions.Clock;
using BookingApp.Application.Bookings.AddBooking;
using BookingApp.Application.UnitTests.Infrastructure;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Application.UnitTests.Bookings;

public class AddBookingCommandHandlerTests
{
    private static readonly DateTime UtcNow = new(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc);

    private readonly IConferenceHallRepository _hallRepositoryMock;
    private readonly IBookingRepository _bookingRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AddBookingCommandHandler _handler;

    public AddBookingCommandHandlerTests()
    {
        _hallRepositoryMock = Substitute.For<IConferenceHallRepository>();
        _bookingRepositoryMock = Substitute.For<IBookingRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        dateTimeProviderMock.UtcNow.Returns(UtcNow);

        _handler = new AddBookingCommandHandler(
            _hallRepositoryMock,
            _bookingRepositoryMock,
            new PricingService(),
            dateTimeProviderMock,
            _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenHallIsNotFound()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = CreateCommand();

        _hallRepositoryMock
            .GetByIdAsync(command.HallId, cancellationToken)
            .Returns((ConferenceHall?)null);

        // Act
        Result<BookingConfirmationResponse> result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Error.Should().Be(ConferenceHallErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenBookingStartsInPast()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = CreateCommand(date: DateOnly.FromDateTime(UtcNow), startTime: "06:00", endTime: "07:00");
        ConferenceHall hall = HallData.Create(command.HallId);

        _hallRepositoryMock
            .GetByIdAsync(command.HallId, cancellationToken)
            .Returns(hall);

        // Act
        Result<BookingConfirmationResponse> result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Error.Should().Be(BookingErrors.StartsInPast);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenBookingOverlaps()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = CreateCommand();
        ConferenceHall hall = HallData.Create(command.HallId);

        _hallRepositoryMock
            .GetByIdAsync(command.HallId, cancellationToken)
            .Returns(hall);

        _bookingRepositoryMock
            .HasOverlap(command.HallId, Arg.Any<DateRange>(), cancellationToken)
            .Returns(true);

        // Act
        Result<BookingConfirmationResponse> result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Error.Should().Be(BookingErrors.Overlap);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenAmenityIsNotSupported()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = CreateCommand(amenities: [Amenity.SoundSystem]);
        ConferenceHall hall = HallData.Create(command.HallId, amenities: [Amenity.Projector]);

        _hallRepositoryMock
            .GetByIdAsync(command.HallId, cancellationToken)
            .Returns(hall);

        _bookingRepositoryMock
            .HasOverlap(command.HallId, Arg.Any<DateRange>(), cancellationToken)
            .Returns(false);

        // Act
        Result<BookingConfirmationResponse> result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Error.Code.Should().Be("Booking.UnsupportedAmenity");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenBookingIsReserved()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = CreateCommand(amenities: [Amenity.Projector, Amenity.WiFi]);
        ConferenceHall hall = HallData.Create(command.HallId);

        _hallRepositoryMock
            .GetByIdAsync(command.HallId, cancellationToken)
            .Returns(hall);

        _bookingRepositoryMock
            .HasOverlap(command.HallId, Arg.Any<DateRange>(), cancellationToken)
            .Returns(false);

        // Act
        Result<BookingConfirmationResponse> result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HallId.Should().Be(command.HallId);
        result.Value.Currency.Should().Be("UAH");
        result.Value.TotalPrice.Should().BeGreaterThan(0m);

        _bookingRepositoryMock.Received(1).Add(Arg.Is<Booking>(booking =>
            booking.Id == result.Value.BookingId &&
            booking.ConferenceHallId == command.HallId));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    private static AddBookingCommand CreateCommand(
        DateOnly? date = null,
        string startTime = "10:40",
        string endTime = "12:10",
        IReadOnlyCollection<Amenity>? amenities = null)
    {
        return new AddBookingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            date ?? DateOnly.FromDateTime(UtcNow.AddDays(1)),
            startTime,
            endTime,
            amenities ?? [Amenity.Projector]);
    }
}
