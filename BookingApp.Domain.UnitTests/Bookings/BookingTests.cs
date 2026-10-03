using BookingApp.Domain.Bookings;
using BookingApp.Domain.Bookings.Events;
using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;
using BookingApp.Domain.Users;
using BookingApp.Domain.UnitTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BookingApp.Domain.UnitTests.Bookings;

public class BookingTests : BaseTest
{
    [Fact]
    public void Reserve_Should_RaiseBookingReservedDomainEvent()
    {
        // Arrange
        var user = User.Create(new FirstName("John"), new LastName("Doe"), new Email("john@doe.com"));
        var price = new Money(100.0m, Currency.Uah);
        var duration = DateRange.Create(new DateTime(2026, 7, 20, 10, 0, 0), new DateTime(2026, 7, 20, 12, 0, 0));
        var hall = new ConferenceHall(Guid.NewGuid(), new Name("Test Hall"), new Capacity(10), price, []);
        var pricingService = new PricingService();

        // Act
        var booking = Booking.Reserve(hall, [], user.Id, duration, DateTime.UtcNow, pricingService);

        // Assert
        BookingReservedDomainEvent bookingReservedDomainEvent = AssertDomainEventWasPublished<BookingReservedDomainEvent>(booking);

        bookingReservedDomainEvent.BookingId.Should().Be(booking.Id);
    }

    [Fact]
    public void Reject_Should_RaiseBookingRejectedDomainEvent_WhenStatusIsReserved()
    {
        // Arrange
        var user = User.Create(new FirstName("John"), new LastName("Doe"), new Email("john@doe.com"));
        var price = new Money(100.0m, Currency.Uah);
        var duration = DateRange.Create(new DateTime(2026, 7, 20, 10, 0, 0), new DateTime(2026, 7, 20, 12, 0, 0));
        var hall = new ConferenceHall(Guid.NewGuid(), new Name("Test Hall"), new Capacity(10), price,[]);
        var booking = Booking.Reserve(hall, [], user.Id, duration, DateTime.UtcNow, new PricingService());

        // Act
        var result = booking.Reject(DateTime.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        BookingRejectedDomainEvent domainEvent = AssertDomainEventWasPublished<BookingRejectedDomainEvent>(booking);
        domainEvent.BookingId.Should().Be(booking.Id);
    }

    [Fact]
    public void Complete_Should_RaiseBookingCompletedDomainEvent_WhenStatusIsReserved()
    {
        // Arrange
        var user = User.Create(new FirstName("John"), new LastName("Doe"), new Email("john@doe.com"));
        var price = new Money(100.0m, Currency.Uah);
        var duration = DateRange.Create(new DateTime(2026, 7, 20, 10, 0, 0), new DateTime(2026, 7, 20, 12, 0, 0));
        var hall = new ConferenceHall(Guid.NewGuid(), new Name("Test Hall"), new Capacity(10), price, []);
        var booking = Booking.Reserve(hall, [], user.Id, duration, DateTime.UtcNow, new PricingService());

        // Act
        var result = booking.Complete(DateTime.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        BookingCompletedDomainEvent domainEvent = AssertDomainEventWasPublished<BookingCompletedDomainEvent>(booking);
        domainEvent.BookingId.Should().Be(booking.Id);
    }

    [Fact]
    public void Cancel_Should_RaiseBookingCancelledDomainEvent_WhenStatusIsReservedAndNotStarted()
    {
        // Arrange
        var user = User.Create(new FirstName("John"), new LastName("Doe"), new Email("john@doe.com"));
        var price = new Money(100.0m, Currency.Uah);
        var duration = DateRange.Create(new DateTime(2026, 7, 20, 10, 0, 0), new DateTime(2026, 7, 20, 12, 0, 0));
        var hall = new ConferenceHall(Guid.NewGuid(), new Name("Test Hall"), new Capacity(10), price, []);
        var booking = Booking.Reserve(hall, [], user.Id, duration, DateTime.UtcNow, new PricingService());
        var utcNow = new DateTime(2026, 7, 20, 9, 0, 0);

        // Act
        var result = booking.Cancel(utcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        BookingCancelledDomainEvent domainEvent = AssertDomainEventWasPublished<BookingCancelledDomainEvent>(booking);
        domainEvent.BookingId.Should().Be(booking.Id);
    }
}
