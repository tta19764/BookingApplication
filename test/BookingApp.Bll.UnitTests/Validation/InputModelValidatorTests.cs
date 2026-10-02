using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared.Models;
using BookingApp.Bll.Managers.Bookings.Validation;
using BookingApp.Bll.Managers.ConferenceHalls.Validation;
using BookingApp.Bll.Managers.Shared.Validation;
using FluentAssertions;

namespace BookingApp.Bll.UnitTests.Validation;

public sealed class InputModelValidatorTests
{
    [Fact]
    public async Task CreateBookingValidator_Should_RejectInvalidPeriodAndIdentifiers()
    {
        // Arrange
        var validator = new CreateBookingModelValidator();
        var model = new CreateBookingModel(Guid.Empty, Guid.Empty, default,
            new TimeOnly(5, 0), new TimeOnly(4, 0), []);

        // Act
        var result = await validator.ValidateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(error => error.PropertyName).Should().Contain([
            nameof(CreateBookingModel.HallId),
            nameof(CreateBookingModel.UserId),
            nameof(CreateBookingModel.Date),
            nameof(CreateBookingModel.StartTime),
            nameof(CreateBookingModel.EndTime)
        ]);
    }

    [Fact]
    public async Task CreateHallValidator_Should_AcceptValidModel()
    {
        // Arrange
        var validator = new CreateHallModelValidator();
        var model = new CreateHallModel("Hall A", 50, 2000m, "UAH", [Amenity.Projector]);

        // Act
        var result = await validator.ValidateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateHallValidator_Should_RejectInvalidEditableValues()
    {
        // Arrange
        var validator = new UpdateHallModelValidator();
        var model = new UpdateHallModel(Guid.Empty, string.Empty, 0, 0, []);

        // Act
        var result = await validator.ValidateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
    }

    [Fact]
    public async Task FindAvailableHallsValidator_Should_RejectEndBeforeStart()
    {
        // Arrange
        var validator = new FindAvailableHallsModelValidator();
        var model = new FindAvailableHallsModel(DateOnly.FromDateTime(DateTime.UtcNow),
            new TimeOnly(14, 0), new TimeOnly(12, 0), 50);

        // Act
        var result = await validator.ValidateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(FindAvailableHallsModel.EndTime));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task PaginationValidator_Should_RejectValuesOutsideSupportedRange(int page, int pageSize)
    {
        // Arrange
        var validator = new PaginationModelValidator();
        var model = new PaginationModel(page, pageSize);

        // Act
        var result = await validator.ValidateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task HallReferenceValidator_Should_RejectEmptyIdentifier()
    {
        // Arrange
        var validator = new HallReferenceModelValidator();
        var model = new HallReferenceModel(Guid.Empty);

        // Act
        var result = await validator.ValidateAsync(model, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
