using BookingApp.Bll.Common.Initialization;
using BookingApp.Services.Web.Configuration;
using Microsoft.Extensions.Options;

namespace BookingApp.Services.Web.Services.Initialization;

/// <summary>Runs configured database initialization and data seeding through its Common interface before the web host starts.</summary>
/// <param name="scopes">Creates the scope owning the DAL seeder.</param>
/// <param name="options">Controls reference and optional demo seeding.</param>
/// <param name="logger">Records committed counts without connection details.</param>
public sealed class StartupDataSeeder(IServiceScopeFactory scopes, IOptions<DatabaseSeedingOptions> options,
    ILogger<StartupDataSeeder> logger)
{
    /// <summary>Applies required scripts, then seeds reference and optional demo data only when enabled.</summary>
    /// <param name="cancellationToken">Cancels database seeding and startup.</param>
    /// <returns>A task completed before accepting requests; seed failures propagate and prevent startup.</returns>
    /// <remarks>Disabled seeding does not resolve IDatabaseSeeder or require seeding credentials. Unchanged applied scripts are skipped; script failures prevent data seeding.</remarks>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled) return;

        await using var scope = scopes.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        await seeder.InitializeAsync(cancellationToken);
        logger.LogInformation("Database initialization scripts verified or applied");
        var reference = await seeder.SeedReferenceDataAsync(cancellationToken);
        logger.LogInformation("Reference data seeding completed: {InsertedRows} rows inserted", reference.InsertedRows);
        if (options.Value.IncludeDemoData)
        {
            var demo = await seeder.SeedDemoDataAsync(cancellationToken);
            logger.LogInformation("Demo data seeding completed: {InsertedRows} rows inserted, skipped: {Skipped}",
                demo.InsertedRows, demo.Skipped);
        }
    }
}
