using BookingApp.Bll.Managers.Bookings;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using FluentAssertions;
using Xunit;

namespace BookingApp.Bll.UnitTests.Bookings;

public class PricingManagerTests
{
    private readonly PricingManager _sut = new();
    private readonly ConferenceHall _hall = new(
        Guid.NewGuid(),
        new Name("Test Hall"),
        new Capacity(100),
        new Money(100, Currency.Uah),
        [
            Amenity.Projector,
            Amenity.WiFi
        ]);

    [Theory]
    [InlineData(100, 14, 0, 1, 1.67)]
    [InlineData(0.30, 14, 0, 1, 0.01)]
    [InlineData(0.30, 13, 59, 2, 0.01)]
    public void CalculatePrice_RoundsCompletedComponentsBeforeAddingTotal(
        decimal rate, int hour, int minute, int minutes, decimal expected)
    {
        var hall = new ConferenceHall(Guid.NewGuid(), new Name("Rounding"), new Capacity(10),
            new Money(rate, Currency.Uah), [Amenity.Projector]);
        var start = new DateTime(2026, 7, 20, hour, minute, 0);
        var result = _sut.CalculatePrice(hall, DateRange.Create(start, start.AddMinutes(minutes)),
            [Amenity.Projector]);

        result.PriceForPeriod.Amount.Should().Be(expected);
        result.AmenitiesUpCharge.Amount.Should().Be(500m);
        result.TotalPrice.Amount.Should().Be(expected + 500m);
    }

    [Fact]
    public void CalculatePrice_ShouldReturnStandardPrice_WhenTimeIsStandard()
    {
        // Arrange: 14:00 - 15:00 (Standard)
        var start = new DateTime(2026, 7, 20, 14, 0, 0);
        var end = start.AddHours(1);
        var period = DateRange.Create(start, end);

        // Act
        var result = _sut.CalculatePrice(_hall, period, [Amenity.Projector]);

        // Assert
        result.TotalPrice.Amount.Should().Be(600);
        result.TotalPrice.Currency.Should().Be(Currency.Uah);
    }

    [Fact]
    public void CalculatePrice_ShouldApplyDiscount_WhenTimeIsMorning()
    {
        // Arrange: 07:00 - 08:00 (-10%)
        var start = new DateTime(2026, 7, 20, 7, 0, 0);
        var end = start.AddHours(1);
        var period = DateRange.Create(start, end);

        // Act
        var result = _sut.CalculatePrice(_hall, period);

        // Assert
        result.TotalPrice.Amount.Should().Be(90);
    }

    [Fact]
    public void CalculatePrice_ShouldApplySurcharge_WhenTimeIsLunch()
    {
        // Arrange: 12:00 - 13:00 (+15%)
        var start = new DateTime(2026, 7, 20, 12, 0, 0);
        var end = start.AddHours(1);
        var period = DateRange.Create(start, end);

        // Act
        var result = _sut.CalculatePrice(_hall, period);

        // Assert
        result.TotalPrice.Amount.Should().Be(115);
    }

    [Fact]
    public void CalculatePrice_ShouldApplyDiscount_WhenTimeIsEvening()
    {
        // Arrange: 19:00 - 20:00 (-20%)
        var start = new DateTime(2026, 7, 20, 19, 0, 0);
        var end = start.AddHours(1);
        var period = DateRange.Create(start, end);

        // Act
        var result = _sut.CalculatePrice(_hall, period);

        // Assert
        result.TotalPrice.Amount.Should().Be(80);
    }

    [Fact]
    public void CalculatePrice_ShouldCalculateCorrectly_WhenSpanningMultiplePeriods()
    {
        // Arrange: 08:00 - 10:00
        // 08:00 - 09:00: 100 * 0.90 = 90
        // 09:00 - 10:00: 100 * 1.00 = 100
        // Total: 190
        var start = new DateTime(2026, 7, 20, 8, 0, 0);
        var end = start.AddHours(2);
        var period = DateRange.Create(start, end);

        // Act
        var result = _sut.CalculatePrice(_hall, period, [Amenity.WiFi]);

        // Assert
        result.TotalPrice.Amount.Should().Be(490);
    }

    [Fact]
    public void CalculatePrice_ShouldThrowException_WhenTimeIsOutsideAllowedHours()
    {
        // Arrange: 05:00 - 06:00 is outside allowed rental hours.
        var start = new DateTime(2026, 7, 20, 5, 0, 0);
        var end = start.AddHours(1);
        var period = DateRange.Create(start, end);

        // Act
        var act = () => _sut.CalculatePrice(_hall, period);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Bookings are allowed only between 06:00 and 23:00.");
    }

    [Fact]
    public void CalculatePrice_ShouldCalculateFractionalHours_WhenPeriodUsesMinutes()
    {
        // Arrange: 10:40 - 12:10 spans 80 standard minutes and 10 peak minutes.
        var start = new DateTime(2026, 7, 20, 10, 40, 0);
        var end = new DateTime(2026, 7, 20, 12, 10, 0);
        var period = DateRange.Create(start, end);

        // Act
        var result = _sut.CalculatePrice(_hall, period);

        // Assert
        result.TotalPrice.Amount.Should().Be(152.50m);
    }

    [Fact]
    public void CalculatePrice_ShouldThrowException_WhenPeriodUsesSeconds()
    {
        // Arrange
        var start = new DateTime(2026, 7, 20, 10, 40, 30);
        var end = new DateTime(2026, 7, 20, 12, 0, 0);
        var period = DateRange.Create(start, end);

        // Act
        var act = () => _sut.CalculatePrice(_hall, period);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Bookings must start and end at minute precision.");
    }

    [Fact]
    public void CalculatePrice_ShouldThrowException_WhenPeriodCrossesCalendarDay()
    {
        // Arrange
        var start = new DateTime(2026, 7, 20, 22, 0, 0);
        var end = new DateTime(2026, 7, 21, 7, 0, 0);
        var period = DateRange.Create(start, end);

        // Act
        var act = () => _sut.CalculatePrice(_hall, period);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Booking period must be within one calendar day.");
    }

    [Fact]
    public void DateRangeCreate_ShouldCreateMinuteLevelRange_FromDateAndTimes()
    {
        // Arrange
        var date = new DateOnly(2026, 7, 20);

        // Act
        var period = DateRange.Create(date, new TimeOnly(10, 40), new TimeOnly(14, 15));

        // Assert
        period.Start.Should().Be(new DateTime(2026, 7, 20, 10, 40, 0));
        period.End.Should().Be(new DateTime(2026, 7, 20, 14, 15, 0));
    }

    [Fact]
    public void CalculatePrice_ShouldThrowException_WhenInvalidAmenityIsProvided()
    {
        // Arrange: 23:00 - 01:00 (Next day) - This is outside allowed 06:00-23:00
        var start = new DateTime(2026, 7, 20, 23, 0, 0);
        var end = start.AddHours(2);
        var period = DateRange.Create(start, end);

        // Act
        var act = () => _sut.CalculatePrice(_hall, period, [Amenity.WiFi, Amenity.SoundSystem]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Hall '{_hall.Name}' does not support '{Amenity.SoundSystem}'.");
    }
}
