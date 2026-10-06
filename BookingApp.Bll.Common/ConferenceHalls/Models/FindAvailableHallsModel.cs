namespace BookingApp.Bll.Common.ConferenceHalls.Models;

public sealed record FindAvailableHallsModel(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Capacity);
