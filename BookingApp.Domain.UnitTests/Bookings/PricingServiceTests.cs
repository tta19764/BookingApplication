using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;
using FluentAssertions;
using Xunit;

namespace BookingApp.Domain.UnitTests.Bookings;

public class PricingServiceTests
{
    private readonly PricingService _sut = new();
    private readonly ConferenceHall _hall = new(
        Guid.NewGuid(),
        new Name("Test Hall"),
        new Capacity(100),
        new Money(100, Currency.Uah),
        [
            Amenity.Projector,
            Amenity.WiFi
        ]);

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
        // Arrange: 23:00 - 01:00 (Next day) - This is outside allowed 06:00-23:00
        var start = new DateTime(2026, 7, 20, 23, 0, 0);
        var end = start.AddHours(2);
        var period = DateRange.Create(start, end);

        // Act
        var act = () => _sut.CalculatePrice(_hall, period);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Bookings are allowed only between 06:00 and 23:00.");
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