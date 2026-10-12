using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Services.Web.Dtos.Requests;

/// <summary>
/// Request body for creating a booking for the seeded user.
/// </summary>
/// <param name="HallId">Identifier of the conference hall.</param>
/// <param name="Date">UTC calendar date of the rental.</param>
/// <param name="StartTime">Rental start time on the specified UTC date.</param>
/// <param name="EndTime">Rental end time on the specified UTC date.</param>
/// <param name="Amenities">Amenities included in the hall or requested for the booking.</param>
public sealed record CreateBookingRequest(
    Guid HallId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    IReadOnlyCollection<Amenity> Amenities)
{
    /// <summary>Identifier of the conference hall.</summary>
    public Guid HallId { get; init; } = HallId;

    /// <summary>UTC calendar date of the rental.</summary>
    public DateOnly Date { get; init; } = Date;

    /// <summary>Rental start time on the specified UTC date.</summary>
    public TimeOnly StartTime { get; init; } = StartTime;

    /// <summary>Rental end time on the specified UTC date.</summary>
    public TimeOnly EndTime { get; init; } = EndTime;

    /// <summary>Amenities included in the hall or requested for the booking.</summary>
    public IReadOnlyCollection<Amenity> Amenities { get; init; } = Amenities;
}
