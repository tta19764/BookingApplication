# Azure Key Vault configuration

Tests run in the `Testing` environment, whose appsettings disable Key Vault through
`KeyVault:Enabled=false`. The integration factory also supplies this override and
disables Telegram, keeping tests on their ephemeral SQL Server rather than Azure.
The flag defaults to true in other environments when omitted.

`KeyVaultConfiguration.AddKeyVaultSecrets` runs before logging and service registration.
When `KeyVault:Uri` is blank or absent, the app uses its existing configuration sources.
When configured, secrets from the vault override matching JSON, environment variable,
and command-line settings. Authentication or network failures prevent startup rather
than silently using another secret. Secrets load at startup; restart after changing them.

## Local development

1. In the vault, create the following secrets. Use two dashes for configuration section separators:

   | Key Vault secret | Application configuration |
   | --- | --- |
   | `ConnectionStrings--Database` | `ConnectionStrings:Database` |
   | `ConnectionStrings--Seeding` (optional) | `ConnectionStrings:Seeding` |
   | `Telegram--BotToken` (if notifications are enabled) | `Telegram:BotToken` |

2. Assign your developer account **Key Vault Secrets User** at vault scope under Azure RBAC.
   **Key Vault Secrets Officer**, if already assigned, also permits reading secrets.
   Ensure your computer can reach the vault through its networking rules.

3. Install Azure CLI if needed, then sign in to the vault's tenant:

   ```powershell
   az login --tenant YOUR-TENANT-ID
   ```

   Development uses `DefaultAzureCredential`, which can authenticate through Azure CLI
   or a signed-in Visual Studio account. The selected account must have vault access.
   Host-side Azure CLI credentials are not automatically available inside Docker.

4. From the repository root, set the vault URL and run in Development:

   ```powershell
   $env:KeyVault__Uri = "https://YOUR-VAULT-NAME.vault.azure.net/"
   dotnet run --project BookingApp.Services.Web -- --environment Development
   ```

   Alternatively, set `KeyVault:Uri` in the ignored `appsettings.Development.json`:

   ```json
   "KeyVault": {
     "Uri": "https://YOUR-VAULT-NAME.vault.azure.net/"
   }
   ```

The existing `GetConnectionString("Database")` and Telegram options binding read the
loaded secrets automatically. Keep `Telegram:ChatId`, `Telegram:MessageThreadId`, and
`Telegram:Enabled` in ordinary configuration. Never log secret values.

To use local connection strings instead, remove the vault URI from JSON and remove
the environment variable (`Remove-Item Env:KeyVault__Uri -ErrorAction SilentlyContinue`).

## Azure deployment

Outside Development, this implementation uses `ManagedIdentityCredential` with a
system-assigned identity. Enable that identity on your Azure host, grant it **Key Vault
Secrets User** at vault scope, allow its network access, and configure `KeyVault__Uri`.
Your personal role assignment does not authorize the deployed app.

## Troubleshooting

- Credential unavailable: run `az login` and ensure the app runs in Development.
- Forbidden: check the caller's role, role propagation, tenant, and vault networking.
- Missing connection string: check the exact `ConnectionStrings--Database` secret name.
- Database connection failure after secrets load: database credentials and SQL Server
  network permissions are separate from Key Vault access.

References: [ASP.NET Core Key Vault provider](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-10.0)
and [Key Vault RBAC](https://learn.microsoft.com/en-us/azure/key-vault/general/rbac-guide).
