using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Dal.SqlServerRepositories.Repositories;
using FluentAssertions;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

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
}
