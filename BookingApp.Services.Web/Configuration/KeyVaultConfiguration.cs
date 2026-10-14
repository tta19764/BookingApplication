using Azure.Core;
using Azure.Identity;

namespace BookingApp.Services.Web.Configuration;

/// <summary>Loads Key Vault secrets before application services read configuration.</summary>
public static class KeyVaultConfiguration
{
    /// <summary>
    /// Adds secrets when KeyVault:Uri is configured. Development uses the signed-in developer;
    /// other environments use the host's system-assigned managed identity.
    /// Key Vault values override matching values from previously registered providers.
    /// </summary>
    public static WebApplicationBuilder AddKeyVaultSecrets(this WebApplicationBuilder builder)
    {
        var vaultUri = builder.Configuration["KeyVault:Uri"];
        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            return builder;
        }

        if (!Uri.TryCreate(vaultUri, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("KeyVault:Uri must be an absolute HTTPS URL.");
        }

        TokenCredential credential = builder.Environment.IsDevelopment()
            ? new DefaultAzureCredential()
            : new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);

        builder.Configuration.AddAzureKeyVault(uri, credential);
        return builder;
    }
}
