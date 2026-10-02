using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Dal.SqlRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Bll.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected readonly IManagerDispatcher Sender;
    protected readonly ApplicationDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        Sender = _scope.ServiceProvider.GetRequiredService<IManagerDispatcher>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
