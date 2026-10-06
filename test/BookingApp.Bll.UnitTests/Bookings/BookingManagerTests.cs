using AutoMapper;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Errors;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared.Events;
using BookingApp.Bll.UnitTests.Infrastructure;
using BookingApp.Bll.Managers.Bookings.Validation;
using BookingApp.Bll.Managers.Shared.Validation;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Bll.UnitTests.Bookings;

public class BookingManagerTests
{
    [Theory]
    [InlineData(ReservationOutcome.Overlap, "Booking.Overlap")]
    [InlineData(ReservationOutcome.HallNotFound, "ConferenceHall.NotFound")]
    [InlineData(ReservationOutcome.UserNotFound, "User.NotFound")]
    public async Task AddBookingAsync_MapsAtomicConflict_WithoutDispatchingEvent(ReservationOutcome outcome, string code)
    {
        var halls = Substitute.For<IConferenceHallRepository>();
        var bookings = Substitute.For<IBookingRepository>();
        var events = Substitute.For<IDomainEventDispatcher>();
        var now = new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc);
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(new DateTimeOffset(now));
        var hall = HallData.Create(Guid.NewGuid());
        var token = TestContext.Current.CancellationToken;
        halls.GetByIdAsync(hall.Id, token).Returns(hall);
        bookings.CreateReservationAsync(Arg.Any<Booking>(), token).Returns(outcome);
        var manager = new BookingManager(halls, bookings, new PricingManager(), clock, events,
            Substitute.For<IMapper>(), new PaginationModelValidator(), new CreateBookingModelValidator());
        var result = await manager.AddBookingAsync(new CreateBookingModel(hall.Id, Guid.NewGuid(),
            DateOnly.FromDateTime(now.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(11, 0), []), token);
        result.Error.Code.Should().Be(code);
        await events.DidNotReceive().DispatchAsync(Arg.Any<BookingCreatedDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddBookingAsync_DoesNotMislabelPersistenceFailureAsInvalidPeriod()
    {
        var halls = Substitute.For<IConferenceHallRepository>();
        var bookings = Substitute.For<IBookingRepository>();
        var events = Substitute.For<IDomainEventDispatcher>();
        var now = new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc);
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(new DateTimeOffset(now));
        var hall = HallData.Create(Guid.NewGuid());
        var token = TestContext.Current.CancellationToken;
        halls.GetByIdAsync(hall.Id, token).Returns(hall);
        bookings.CreateReservationAsync(Arg.Any<Booking>(), token)
            .Returns(Task.FromException<ReservationOutcome>(new InvalidOperationException("Database failure")));
        var manager = new BookingManager(halls, bookings, new PricingManager(), clock, events,
            Substitute.For<IMapper>(), new PaginationModelValidator(), new CreateBookingModelValidator());
        Func<Task> create = async () => await manager.AddBookingAsync(new CreateBookingModel(hall.Id, Guid.NewGuid(),
            DateOnly.FromDateTime(now.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(11, 0), []), token);
        await create.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database failure");
        await events.DidNotReceive().DispatchAsync(Arg.Any<BookingCreatedDomainEvent>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task AddBookingAsync_ReturnsFailure_WhenHallDoesNotExist()
    {
        // Arrange
        var halls = Substitute.For<IConferenceHallRepository>();
        var bookings = Substitute.For<IBookingRepository>();
        var clock = Substitute.For<TimeProvider>();
        var events = Substitute.For<IDomainEventDispatcher>();
        var manager = new BookingManager(halls, bookings, new PricingManager(), clock, events,
            Substitute.For<IMapper>(), new PaginationModelValidator(), new CreateBookingModelValidator());
        var hallId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        halls.GetByIdAsync(hallId, cancellationToken).Returns((ConferenceHall?)null);

        // Act
        var model = new CreateBookingModel(hallId, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0), new TimeOnly(11, 0), []);
        Result<BookingConfirmationModel> result = await manager.AddBookingAsync(model, cancellationToken);

        // Assert
        result.Error.Should().Be(ConferenceHallErrors.NotFound);
    }

    [Fact]
    public async Task AddBookingAsync_PersistsBooking_WhenRequestIsValid()
    {
        // Arrange
        var halls = Substitute.For<IConferenceHallRepository>();
        var bookings = Substitute.For<IBookingRepository>();
        var clock = Substitute.For<TimeProvider>();
        var events = Substitute.For<IDomainEventDispatcher>();
        var manager = new BookingManager(halls, bookings, new PricingManager(), clock, events,
            Substitute.For<IMapper>(), new PaginationModelValidator(), new CreateBookingModelValidator());
        var hall = HallData.Create(Guid.NewGuid());
        var now = new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc);
        var cancellationToken = TestContext.Current.CancellationToken;
        clock.GetUtcNow().Returns(new DateTimeOffset(now));
        halls.GetByIdAsync(hall.Id, cancellationToken).Returns(hall);
        bookings.HasOverlapAsync(hall.Id, Arg.Any<DateRange>(), cancellationToken).Returns(false);

        // Act
        var model = new CreateBookingModel(hall.Id, Guid.NewGuid(), DateOnly.FromDateTime(now.AddDays(1)),
            new TimeOnly(10, 0), new TimeOnly(11, 0), [Amenity.Projector]);
        Result<BookingConfirmationModel> result = await manager.AddBookingAsync(model, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await bookings.Received(1).CreateReservationAsync(Arg.Is<Booking>(booking => booking.Id == result.Value.BookingId), cancellationToken);
        await events.Received(1).DispatchAsync(
            Arg.Is<BookingCreatedDomainEvent>(domainEvent => domainEvent.BookingId == result.Value.BookingId),
            cancellationToken);
    }
}
