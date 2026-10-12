namespace BookingApp.Bll.Common.ConferenceHalls.Models;

/// <summary>Identifier of a conference hall for a query or mutation.</summary>
/// <param name="HallId">Identifier of the conference hall.</param>
public sealed record HallReferenceModel(Guid HallId)
{
    /// <summary>Identifier of the conference hall.</summary>
    public Guid HallId { get; init; } = HallId;
}
