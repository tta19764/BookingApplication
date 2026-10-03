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
    [Fact]
    public async Task AddBookingAsync_ReturnsFailure_WhenHallDoesNotExist()
    {
        // Arrange
        var halls = Substitute.For<IConferenceHallRepository>();
        var bookings = Substitute.For<IBookingRepository>();
        var clock = Substitute.For<TimeProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var events = Substitute.For<IDomainEventDispatcher>();
        var manager = new BookingManager(halls, bookings, new PricingManager(), clock, unitOfWork, events,
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
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var events = Substitute.For<IDomainEventDispatcher>();
        var manager = new BookingManager(halls, bookings, new PricingManager(), clock, unitOfWork, events,
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
        bookings.Received(1).Add(Arg.Is<Booking>(booking => booking.Id == result.Value.BookingId));
        halls.Received(1).Update(hall);
        await unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        await events.Received(1).DispatchAsync(
            Arg.Is<BookingCreatedDomainEvent>(domainEvent => domainEvent.BookingId == result.Value.BookingId),
            cancellationToken);
    }
}
