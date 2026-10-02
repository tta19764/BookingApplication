namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class EntityChangeTracker
{
    private readonly List<Action> _synchronizers = [];
    private readonly List<BookingApp.Bll.Common.Abstractions.IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<BookingApp.Bll.Common.Abstractions.IDomainEvent> DomainEvents => _domainEvents;

    public void Track(Action synchronizer) => _synchronizers.Add(synchronizer);

    public void TrackEvents(BookingApp.Bll.Common.Abstractions.Entity model)
    {
        _domainEvents.AddRange(model.GetDomainEvents());
        model.ClearDomainEvents();
    }

    public void ClearEvents() => _domainEvents.Clear();

    public void Apply()
    {
        foreach (var synchronizer in _synchronizers.ToList())
        {
            synchronizer();
        }

        _synchronizers.Clear();
    }
}
