using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.ConferenceHalls.UpdateHall;

public class UpdateHallCommandHandler : ICommandHandler<UpdateHallCommand>
{
    public Task<Result> Handle(UpdateHallCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}