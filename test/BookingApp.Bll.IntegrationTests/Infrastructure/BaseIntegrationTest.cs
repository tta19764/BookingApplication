using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Bll.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IDisposable
{
    private readonly IServiceScope _scope;

    protected readonly IBookingManager BookingManager;
    protected readonly IConferenceHallManager HallManager;
    protected readonly IBookingRepository Bookings;
    protected readonly IConferenceHallRepository Halls;
    protected readonly IntegrationTestWebAppFactory Factory;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        _scope = factory.Services.CreateScope();

        BookingManager = _scope.ServiceProvider.GetRequiredService<IBookingManager>();
        HallManager = _scope.ServiceProvider.GetRequiredService<IConferenceHallManager>();
        Bookings = _scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        Halls = _scope.ServiceProvider.GetRequiredService<IConferenceHallRepository>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
