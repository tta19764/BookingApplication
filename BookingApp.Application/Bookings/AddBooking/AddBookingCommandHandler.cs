using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.Bookings.AddBooking;

public class AddBookingCommandHandler : ICommandHandler<AddBookingCommand, Guid>
{
    public Task<Result<Guid>> Handle(AddBookingCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}