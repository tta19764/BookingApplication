using BookingApp.Bll.Common.Initialization.Models;

namespace BookingApp.Bll.Common.Initialization;

/// <summary>Defines opt-in application initialization without exposing database provider types.</summary>
public interface IDatabaseSeeder
{
    /// <summary>Applies required schema, procedure and permission scripts, skipping unchanged applied versions.</summary>
    /// <param name="cancellationToken">Cancels script initialization.</param>
    /// <returns>A task completed when the database is ready for data seeding.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads existing application data counts without changing data.</summary>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>Existing row counts and whether any data exists.</returns>
    Task<DatabaseInspection> InspectAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds missing required reference data atomically without replacing existing values.</summary>
    /// <param name="cancellationToken">Cancels the seed operation.</param>
    /// <returns>The previous data counts and committed insertion count.</returns>
    Task<SeedResult> SeedReferenceDataAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds optional demo data only when the hall catalog is empty.</summary>
    /// <param name="cancellationToken">Cancels the seed operation.</param>
    /// <returns>The insertion count and whether existing halls caused a skip.</returns>
    Task<SeedResult> SeedDemoDataAsync(CancellationToken cancellationToken = default);
}
