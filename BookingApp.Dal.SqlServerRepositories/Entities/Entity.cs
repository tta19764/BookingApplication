namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Provides a strongly typed primary key for single-key persistence entities.</summary>
/// <typeparam name="TKey">The non-null primary-key type used by the database table.</typeparam>
public abstract class Entity<TKey> where TKey : notnull
{
    /// <summary>Gets or sets the primary key.</summary>
    public TKey Id { get; set; } = default!;
}
