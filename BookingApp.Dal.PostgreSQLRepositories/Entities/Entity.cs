namespace BookingApp.Dal.SqlRepositories.Entities;

/// <summary>
/// Base type for entities persisted by the PostgreSQL DAL.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; set; }
}
