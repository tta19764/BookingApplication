using AutoMapper;
using BookingApp.Services.Web.DI;
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
                ["ConnectionStrings:Database"] = "Server=localhost;Database=test;Integrated Security=True;Encrypt=True"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApi(configuration);

        // Act
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        // Assert
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
