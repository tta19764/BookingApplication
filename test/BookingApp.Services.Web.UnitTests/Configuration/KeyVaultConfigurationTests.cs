using BookingApp.Services.Web.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace BookingApp.Services.Web.UnitTests.Configuration;

public sealed class KeyVaultConfigurationTests
{
    [Fact]
    public void DisabledVaultDoesNotValidateUriOrAuthenticateAndPreservesTestConnection()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["KeyVault:Enabled"] = "false",
            ["KeyVault:Uri"] = "invalid-vault-uri",
            ["ConnectionStrings:Database"] = "test-only-connection"
        });
        var sourceCount = builder.Configuration.Sources.Count;

        var result = builder.AddKeyVaultSecrets();

        Assert.Same(builder, result);
        Assert.Equal(sourceCount, builder.Configuration.Sources.Count);
        Assert.Equal("test-only-connection", builder.Configuration.GetConnectionString("Database"));
    }
}
