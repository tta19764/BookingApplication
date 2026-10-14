namespace BookingApp.Bll.Common.Bookings.Models;

/// <summary>A rental and its price snapshot for the daily report.</summary>
/// <param name="Id">Booking identifier.</param>
/// <param name="HallName">Display name of the rented hall.</param>
/// <param name="StartUtc">Inclusive UTC rental start.</param>
/// <param name="EndUtc">Exclusive UTC rental end.</param>
/// <param name="Status">Persisted booking status.</param>
/// <param name="TotalPrice">Full booking price, without daily proration.</param>
/// <param name="Currency">Currency code for the total price.</param>
public sealed record BookingReportRow(Guid Id, string HallName, DateTime StartUtc,
    DateTime EndUtc, string Status, decimal TotalPrice, string Currency);

