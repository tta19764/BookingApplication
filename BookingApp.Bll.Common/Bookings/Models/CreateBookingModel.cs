using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Bll.Common.Bookings.Models;

/// <summary>Input for reserving a conference hall for a user.</summary>
/// <param name="HallId">Identifier of the conference hall.</param>
/// <param name="UserId">Identifier of the booking owner.</param>
/// <param name="Date">UTC calendar date of the rental.</param>
/// <param name="StartTime">Rental start time on the specified UTC date.</param>
/// <param name="EndTime">Rental end time on the specified UTC date.</param>
/// <param name="Amenities">Amenities included in the hall or requested for the booking.</param>
public sealed record CreateBookingModel(
    Guid HallId,
    Guid UserId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    IReadOnlyCollection<Amenity> Amenities)
{
    /// <summary>Identifier of the conference hall.</summary>
    public Guid HallId { get; init; } = HallId;

    /// <summary>Identifier of the booking owner.</summary>
    public Guid UserId { get; init; } = UserId;

    /// <summary>UTC calendar date of the rental.</summary>
    public DateOnly Date { get; init; } = Date;

    /// <summary>Rental start time on the specified UTC date.</summary>
    public TimeOnly StartTime { get; init; } = StartTime;

    /// <summary>Rental end time on the specified UTC date.</summary>
    public TimeOnly EndTime { get; init; } = EndTime;

    /// <summary>Amenities included in the hall or requested for the booking.</summary>
    public IReadOnlyCollection<Amenity> Amenities { get; init; } = Amenities;
}
