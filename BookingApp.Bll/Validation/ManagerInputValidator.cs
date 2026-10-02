using System.Globalization;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Shared.Exceptions;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.Validation;

internal static class ManagerInputValidator
{
    internal static void ValidateId(Guid id, string propertyName)
    {
        if (id == Guid.Empty)
            throw new ValidationException([new ValidationError(propertyName, "ID is required.")]);
    }

    internal static void ValidatePage(int page, int pageSize)
    {
        var errors = new List<ValidationError>();
        if (page < 1) errors.Add(new ValidationError(nameof(page), "Page must be greater than zero."));
        if (pageSize is < 1 or > 100) errors.Add(new ValidationError(nameof(pageSize), "Page size must be between 1 and 100."));
        ThrowIfInvalid(errors);
    }

    internal static void ValidateHall(string name, int capacity, decimal hourlyRate,
        IReadOnlyCollection<Amenity> amenities, string? currencyCode = null)
    {
        var errors = new List<ValidationError>();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100) errors.Add(new ValidationError(nameof(name), "Name is required and must not exceed 100 characters."));
        if (capacity is < 1 or > 1000) errors.Add(new ValidationError(nameof(capacity), "Capacity must be between 1 and 1000."));
        if (hourlyRate <= 0) errors.Add(new ValidationError(nameof(hourlyRate), "Hourly rate must be greater than zero."));
        if (currencyCode is not null && !Currency.All.Any(currency => string.Equals(currency.Code, currencyCode.Trim(), StringComparison.OrdinalIgnoreCase)))
            errors.Add(new ValidationError(nameof(currencyCode), "Currency code is not supported."));
        if (amenities.Any(amenity => !Enum.IsDefined(amenity))) errors.Add(new ValidationError(nameof(amenities), "An amenity is not supported."));
        ThrowIfInvalid(errors);
    }

    internal static void ValidatePeriod(Guid? hallId, DateOnly date, string startTime, string endTime, int? capacity = null)
    {
        var errors = new List<ValidationError>();
        if (hallId == Guid.Empty) errors.Add(new ValidationError(nameof(hallId), "Hall ID is required."));
        if (date == default) errors.Add(new ValidationError(nameof(date), "Date is required."));
        var startValid = TimeOnly.TryParseExact(startTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start);
        var endValid = TimeOnly.TryParseExact(endTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end);
        if (!startValid || start < new TimeOnly(6, 0) || start >= new TimeOnly(23, 0)) errors.Add(new ValidationError(nameof(startTime), "Start time must use HH:mm format and be between 06:00 and 22:59."));
        if (!endValid || end <= new TimeOnly(6, 0) || end > new TimeOnly(23, 0)) errors.Add(new ValidationError(nameof(endTime), "End time must use HH:mm format and be between 06:01 and 23:00."));
        if (startValid && endValid && start >= end) errors.Add(new ValidationError(nameof(endTime), "End time must be after start time."));
        if (capacity is < 1 or > 1000) errors.Add(new ValidationError(nameof(capacity), "Capacity must be between 1 and 1000."));
        ThrowIfInvalid(errors);
    }

    private static void ThrowIfInvalid(IReadOnlyCollection<ValidationError> errors)
    {
        if (errors.Count > 0) throw new ValidationException(errors);
    }
}
