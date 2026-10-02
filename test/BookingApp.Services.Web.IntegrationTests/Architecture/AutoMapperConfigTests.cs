using AutoMapper;
using BookingApp.Bll;
using BookingApp.Dal.SqlRepositories;
using BookingApp.Services.Web.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class AutoMapperConfigTests
{
    [Fact]
    public void AutoMapperConfig_Should_BeValid()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Database=test;Username=test;Password=test"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApi(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);

        // Act
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        // Assert
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
