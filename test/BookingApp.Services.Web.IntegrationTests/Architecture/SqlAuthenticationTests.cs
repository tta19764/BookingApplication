using FluentAssertions;
using Microsoft.Data.SqlClient;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class SqlAuthenticationTests
{
    [Theory]
    [InlineData(SqlAuthenticationMethod.ActiveDirectoryDefault)]
    [InlineData(SqlAuthenticationMethod.ActiveDirectoryInteractive)]
    [InlineData(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity)]
    public void EntraAuthenticationProvider_IsAvailableWithoutConnecting(SqlAuthenticationMethod method)
    {
        var provider = SqlAuthenticationProvider.GetProvider(method);
        provider.Should().NotBeNull();
        provider!.IsSupported(method).Should().BeTrue();
    }
}
