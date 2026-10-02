using System.Collections;
using BookingApp.Bll.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Bll.Abstractions.Events;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventManager<>).MakeGenericType(domainEvent.GetType());
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            var handlers = (IEnumerable)serviceProvider.GetRequiredService(handlersType);
            var handleMethod = handlerType.GetMethod("Handle")!;

            foreach (var handler in handlers)
            {
                await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
            }
        }
    }
}
