using Azure.Core;
using Azure.Identity;
using BookingApp.AzureFunctions.Services;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Services.Bookings;
using BookingApp.Bll.Services.Shared;
using BookingApp.Dal.SqlServerRepositories;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookingApp.AzureFunctions.Configuration;

/// <summary>Configures Key Vault and the services required for hourly booking reports.</summary>
public static class FunctionsConfiguration
{
    /// <summary>Loads optional vault secrets and registers reporting dependencies when enabled.</summary>
    /// <param name="builder">Functions host builder whose configuration and services are updated.</param>
    /// <exception cref="InvalidOperationException">The vault URL is invalid or an enabled report lacks its database connection string.</exception>
    public static void AddBookingsReporting(this FunctionsApplicationBuilder builder)
    {
        AddKeyVaultSecrets(builder);

        var enabled = builder.Configuration.GetValue<bool>("BookingsReport:Enabled");
        AddReportOptions(builder.Services, builder.Configuration, enabled);

        if (enabled)
        {
            AddReportServices(builder.Services, builder.Configuration);
        }
    }

    private static void AddKeyVaultSecrets(FunctionsApplicationBuilder builder)
    {
        var vaultUri = builder.Configuration["KeyVault:Uri"];
        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            return;
        }

        if (!Uri.TryCreate(vaultUri, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("KeyVault:Uri must be an absolute HTTPS URL.");
        }

        TokenCredential credential = builder.Environment.IsDevelopment()
            ? new DefaultAzureCredential()
            : new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);

        builder.Configuration.AddAzureKeyVault(uri, credential);
    }

    private static void AddReportOptions(IServiceCollection services, IConfiguration configuration, bool enabled)
    {
        services.AddOptions<ReportOptions>()
            .Bind(configuration.GetSection("BookingsReport"))
            .Validate(options => IsValidZone(options.TimeZoneId),
                "BookingsReport:TimeZoneId must identify a valid time zone.")
            .ValidateOnStart();

        services.AddOptions<TelegramOptions>()
            .Bind(configuration.GetSection("Telegram"))
            .Validate(options => !enabled || HasTelegramCredentials(options),
                "Telegram:BotToken and Telegram:ChatId are required when reports are enabled.")
            .Validate(options => options.MessageThreadId is null or > 0,
                "Telegram:MessageThreadId must be positive.")
            .ValidateOnStart();
    }

    private static void AddReportServices(IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connection))
        {
            throw new InvalidOperationException("ConnectionStrings:Database is required when reports are enabled.");
        }

        services.AddSqlServerDataAccess(connection, configuration.GetValue("Database:CommandTimeoutSeconds", 30));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IReportingPeriodService, ReportingPeriodService>();
        services.AddScoped<IBookingReportService, BookingReportService>();
        services.AddSingleton<IBookingsReportFormatter, BookingsReportFormatter>();
        services.AddHttpClient<ITelegramReportSender, TelegramReportSender>(client =>
                client.Timeout = TimeSpan.FromSeconds(15))
            .RemoveAllLoggers();
    }

    private static bool HasTelegramCredentials(TelegramOptions options) =>
        !string.IsNullOrWhiteSpace(options.BotToken) && !string.IsNullOrWhiteSpace(options.ChatId);

    private static bool IsValidZone(string id)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(id);
            return true;
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            return false;
        }
    }
}

