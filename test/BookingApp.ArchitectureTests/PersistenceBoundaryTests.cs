using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Dal.SqlServerRepositories.Repositories;
using FluentAssertions;

namespace BookingApp.ArchitectureTests;

[Trait("Category", "Architecture")]
public sealed class PersistenceBoundaryTests
{
    [Fact]
    public void BusinessAssemblies_DoNotReferenceDatabaseProviders()
    {
        foreach (var assembly in new[] { typeof(IBookingRepository).Assembly, typeof(BookingManager).Assembly })
            assembly.GetReferencedAssemblies().Should().NotContain(reference =>
                reference.Name!.Contains("SqlClient") || reference.Name.Contains("EntityFramework")
                || reference.Name.Contains("Npgsql") || reference.Name.Contains("Dal."));
    }

    [Fact]
    public void Dal_DoesNotReferenceEntityFrameworkOrWeb()
    {
        typeof(BookingRepository).Assembly.GetReferencedAssemblies().Should().NotContain(reference =>
            reference.Name!.Contains("EntityFramework") || reference.Name.Contains("Services.Web"));
    }
    [Fact]
    public void Initialization_EmbedsOnlyApplicationScriptsAndExcludesPersonalDatabaseTypes()
    {
        var assembly = typeof(BookingRepository).Assembly;
        assembly.GetManifestResourceNames().Where(name => name.EndsWith(".sql"))
            .Should().BeEquivalentTo(new[]
            {
                "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.001_schema.sql",
                "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.002_stored_procedures.sql",
                "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.003_permissions.sql",
                "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.004_user_roles_tvp.sql",
                "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.005_tvp_permissions.sql"
            });
        assembly.GetTypes().Should().NotContain(type =>
            (type.Namespace ?? "").StartsWith("BookingApp.Dal.SqlServerRepositories.Database"));
    }
}
