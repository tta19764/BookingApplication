namespace BookingApp.Bll.Common.Initialization.Models;

/// <summary>Describes existing application data without validating or modifying the schema.</summary>
/// <param name="RowCounts">Counts for each of the seven application tables.</param>
public sealed record DatabaseInspection(IReadOnlyDictionary<string, long> RowCounts)
{
    /// <summary>Gets whether any application table contains data.</summary>
    public bool HasData => RowCounts.Values.Any(count => count > 0);
}

/// <summary>Reports the database state before a seed operation and its committed result.</summary>
/// <param name="Before">Application data counts before inserts.</param>
/// <param name="InsertedRows">Number of rows inserted across all affected tables.</param>
/// <param name="Skipped">Whether demo seeding was skipped because halls already existed.</param>
public sealed record SeedResult(DatabaseInspection Before, int InsertedRows, bool Skipped = false);
