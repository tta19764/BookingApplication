using BookingApp.Domain.Bookings;

namespace BookingApp.Domain.ConferenceHalls;

public interface IConferenceHallRepository
{
    Task<ConferenceHall> GetById(Guid id, CancellationToken cancellationToken = default);
    void Add(ConferenceHall hall);
    void Update(ConferenceHall hall);
    void Remove(ConferenceHall hall);
    
    Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHalls(DateRange dateRange, Capacity seats, CancellationToken cancellationToken = default);
}