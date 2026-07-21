namespace BookingApp.Domain.Bookings;

/// <summary>
/// Provides persistence operations for bookings.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Finds a booking by its identifier.
    /// </summary>
    Task<Booking?> GetById(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the hall already has a booking that overlaps the requested period.
    /// </summary>
    Task<bool> HasOverlap(Guid conferenceHallId, DateRange duration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists bookings for reporting and read-side projections.
    /// </summary>
    Task<IReadOnlyCollection<Booking>> List(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new booking to the persistence context.
    /// </summary>
    void Add(Booking booking);

    /// <summary>
    /// Marks an existing booking as changed in the persistence context.
    /// </summary>
    void Update(Booking booking);

    /// <summary>
    /// Removes a booking from the persistence context.
    /// </summary>
    void Remove(Booking booking);
}