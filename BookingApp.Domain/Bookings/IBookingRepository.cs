namespace BookingApp.Domain.Bookings;

public interface IBookingRepository
{
    Task<Booking> GetById(Guid id, CancellationToken cancellationToken = default);
    void Add(Booking booking);
    void Update(Booking booking);
    void Remove(Booking booking);
}