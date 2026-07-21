using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.ConferenceHalls.AddHall;

public class AddHallCommandHandler : ICommandHandler<AddHallCommand, Guid>
{
    public Task<Result<Guid>> Handle(AddHallCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}