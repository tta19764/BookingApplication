using BookingApp.Bll.Common.Initialization.Models;

namespace BookingApp.Bll.Common.Initialization;

/// <summary>Defines opt-in application initialization without exposing database provider types.</summary>
public interface IDatabaseSeeder
{
    /// <summary>Applies the database initialization scripts.</summary>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>A task completed after initialization scripts are applied or verified.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads existing application data counts.</summary>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>Existing application table row counts.</returns>
    Task<DatabaseInspection> InspectAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds missing reference data atomically.</summary>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>The previous data counts and number of committed inserts.</returns>
    Task<SeedResult> SeedReferenceDataAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds sample halls when the hall catalog is empty.</summary>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>The previous data counts, committed insert count, and whether seeding was skipped.</returns>
    Task<SeedResult> SeedDemoDataAsync(CancellationToken cancellationToken = default);
}
