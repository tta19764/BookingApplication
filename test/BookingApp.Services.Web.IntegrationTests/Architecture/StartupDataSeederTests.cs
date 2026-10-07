using BookingApp.Bll.Common.Initialization;
using BookingApp.Dal.SqlServerRepositories.Initialization;
using BookingApp.Bll.Common.Initialization.Models;
using BookingApp.Services.Web.Configuration;
using BookingApp.Services.Web.DI;
using BookingApp.Services.Web.Services.Initialization;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class StartupDataSeederTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private static SeedResult Result => new(new DatabaseInspection(new Dictionary<string, long>()), 0);

    private static IDatabaseSeeder SubstituteSeeder()
    {
        var seeder = Substitute.For<IDatabaseSeeder>();
        seeder.InitializeAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        seeder.SeedReferenceDataAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result));
        seeder.SeedDemoDataAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result));
        seeder.ClearReceivedCalls();
        return seeder;
    }

    private static StartupDataSeeder Startup(ServiceProvider services, bool includeDemo = false) => new(
        services.GetRequiredService<IServiceScopeFactory>(),
        Options.Create(new DatabaseSeedingOptions { Enabled = true, IncludeDemoData = includeDemo }),
        Substitute.For<ILogger<StartupDataSeeder>>());

    [Fact]
    public async Task Disabled_DoesNotResolveSeederOrRequireSeedingCredentials()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApi(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=localhost;Database=Booking;Integrated Security=true",
            ["DatabaseSeeding:Enabled"] = "false",
            ["BackgroundJobs:CompleteBookings:Enabled"] = "false"
        }).Build());
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IDatabaseSeeder)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<StartupDataSeeder>().SeedAsync(Token);
    }

    [Fact]
    public async Task Enabled_SeedsReferenceDataOnlyByDefault()
    {
        var seeder = SubstituteSeeder();
        await using var provider = new ServiceCollection().AddScoped(_ => seeder).BuildServiceProvider();
        await Startup(provider).SeedAsync(Token);
        await seeder.Received(1).InitializeAsync(Token);
        await seeder.Received(1).SeedReferenceDataAsync(Token);
        await seeder.DidNotReceiveWithAnyArgs().SeedDemoDataAsync(Token);
    }

    [Fact]
    public async Task EnabledDemo_SeedsReferenceDataBeforeDemoData()
    {
        var seeder = SubstituteSeeder();
        await using var provider = new ServiceCollection().AddScoped(_ => seeder).BuildServiceProvider();
        await Startup(provider, true).SeedAsync(Token);
        seeder.ReceivedCalls().Select(call => call.GetMethodInfo().Name)
            .Should().Equal(nameof(IDatabaseSeeder.InitializeAsync), nameof(IDatabaseSeeder.SeedReferenceDataAsync), nameof(IDatabaseSeeder.SeedDemoDataAsync));
        await seeder.Received(1).SeedDemoDataAsync(Token);
    }

    [Fact]
    public async Task ScriptFailure_PropagatesAndPreventsAllDataSeeding()
    {
        var seeder = SubstituteSeeder();
        seeder.InitializeAsync(Token).Returns(Task.FromException(new InvalidOperationException("script failure")));
        await using var provider = new ServiceCollection().AddScoped(_ => seeder).BuildServiceProvider();
        var action = () => Startup(provider, true).SeedAsync(Token);
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("script failure");
        await seeder.DidNotReceiveWithAnyArgs().SeedReferenceDataAsync(Token);
        await seeder.DidNotReceiveWithAnyArgs().SeedDemoDataAsync(Token);
    }

    [Fact]
    public async Task ReferenceFailure_PropagatesAndPreventsDemoSeeding()
    {
        var seeder = SubstituteSeeder();
        seeder.SeedReferenceDataAsync(Token).Returns(Task.FromException<SeedResult>(new InvalidOperationException("seed failure")));
        await using var provider = new ServiceCollection().AddScoped(_ => seeder).BuildServiceProvider();
        var action = () => Startup(provider, true).SeedAsync(Token);
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("seed failure");
        await seeder.DidNotReceiveWithAnyArgs().SeedDemoDataAsync(Token);
    }

    [Fact]
    public async Task MissingOrBlankSeedingConnection_FallsBackToDatabaseConnection()
    {
        foreach (var seedingConnection in new string?[] { null, "", "   " })
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApi(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Server=localhost;Database=Booking;Integrated Security=true",
                ["ConnectionStrings:Seeding"] = seedingConnection,
                ["DatabaseSeeding:Enabled"] = "true",
                ["BackgroundJobs:CompleteBookings:Enabled"] = "false"
            }).Build());
            await using var provider = services.BuildServiceProvider();
            await using var scope = provider.CreateAsyncScope();
            // Construction validates the fallback string without opening a database connection.
            scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>().Should().BeOfType<DatabaseSeeder>();
        }
    }

    [Fact]
    public async Task ExplicitSeedingConnection_IsUsedInsteadOfFallback()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApi(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=localhost;Database=Booking;Integrated Security=true",
            ["ConnectionStrings:Seeding"] = "invalid connection string",
            ["BackgroundJobs:CompleteBookings:Enabled"] = "false"
        }).Build());
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var action = () => scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task Cancellation_PropagatesAndPreventsDemoSeeding()
    {
        var seeder = SubstituteSeeder();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        cancellation.Cancel();
        seeder.SeedReferenceDataAsync(cancellation.Token).Returns(Task.FromCanceled<SeedResult>(cancellation.Token));
        await using var provider = new ServiceCollection().AddScoped(_ => seeder).BuildServiceProvider();
        var action = () => Startup(provider, true).SeedAsync(cancellation.Token);
        await action.Should().ThrowAsync<OperationCanceledException>();
        await seeder.DidNotReceiveWithAnyArgs().SeedDemoDataAsync(Token);
    }
}
