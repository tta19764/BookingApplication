namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class EntityChangeTracker
{
    private readonly List<Action> _synchronizers = [];

    public void Track(Action synchronizer) => _synchronizers.Add(synchronizer);


    public void Apply()
    {
        foreach (var synchronizer in _synchronizers.ToList())
        {
            synchronizer();
        }

        _synchronizers.Clear();
    }
}
