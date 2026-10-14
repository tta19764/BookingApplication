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
        var scripts = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)).ToArray();
        scripts.Should().NotBeEmpty();
        scripts.Should().OnlyContain(name => name.StartsWith(
            "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.", StringComparison.Ordinal));
        scripts.Should().OnlyContain(name => System.Text.RegularExpressions.Regex.IsMatch(
            name, @"\.\d{3}_[a-z0-9_]+\.sql$"));
        assembly.GetTypes().Should().NotContain(type =>
            (type.Namespace ?? "").StartsWith("BookingApp.Dal.SqlServerRepositories.Database"));
    }
}
