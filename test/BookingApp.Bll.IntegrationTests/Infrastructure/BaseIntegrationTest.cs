using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Dal.SqlRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Bll.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected readonly IBookingManager BookingManager;
    protected readonly IConferenceHallManager HallManager;
    protected readonly ApplicationDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        BookingManager = _scope.ServiceProvider.GetRequiredService<IBookingManager>();
        HallManager = _scope.ServiceProvider.GetRequiredService<IConferenceHallManager>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
