namespace BookingApp.Services.Web.Configuration;

/// <summary>Controls opt-in script initialization and data seeding before the application starts serving requests.</summary>
public sealed class DatabaseSeedingOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "DatabaseSeeding";

    /// <summary>Gets whether startup initializes scripts and runs reference data seeding; false by default.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets whether enabled startup seeding also includes demo halls; false by default.</summary>
    public bool IncludeDemoData { get; init; }
}
