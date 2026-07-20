namespace BookingApp.Domain.Users;

public sealed class Permission(int id, string name)
{
    public static readonly Permission ConferenceHallRead = new(1, "conference-halls:read");
    public static readonly Permission ConferenceHallWrite = new(2, "conference-halls:write");
    public static readonly Permission BookingRead = new(3, "bookings:read");
    public static readonly Permission BookingWrite = new(4, "bookings:write");

    public int Id { get; init; } = id;

    public string Name { get; init; } = name;
}