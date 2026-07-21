using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.ConferenceHalls.RemoveHall;

public class RemoveHallCommandHandler : ICommandHandler<RemoveHallCommand>
{
    public Task<Result> Handle(RemoveHallCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}