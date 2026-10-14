using Asp.Versioning;
using BookingApp.Bll.Common.Initialization;
using BookingApp.Dal.SqlServerRepositories.Initialization;
using BookingApp.Services.Web.Services.Initialization;
using BookingApp.Services.Web.Configuration;
using BookingApp.Services.Web.Mappings;
using System.Text.Json.Serialization;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Bll.Common.Shared.Events;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Reports;
using BookingApp.Bll.Managers.ConferenceHalls;
using BookingApp.Bll.Managers.Reports;
using BookingApp.Bll.Managers.Bookings.Validation;
using BookingApp.Dal.SqlServerRepositories;
using BookingApp.Services.Web.Services.BackgroundJobs;
using BookingApp.Services.Web.Services.DomainEvents;
using Quartz;
using FluentValidation;
using BookingApp.Services.Web.Services.Telegram;

namespace BookingApp.Services.Web.DI;

/// <summary>
/// Registers API-layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI, JSON settings, and problem details.
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddAutoMapper(_ => { }, typeof(AutoMapperConfig), typeof(BookingApp.Bll.Mappings.AutoMapperConfig),
            typeof(BookingApp.Dal.SqlServerRepositories.Mappings.AutoMapperConfig));

        AddBusinessLogic(services);
        AddTelegramNotifications(services, configuration);
        AddDataAccess(services, configuration);

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = BookingAppApiVersions.V1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddSwaggerGen(options =>
        {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BookingApp.Services.Web.xml"));
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BookingApp.Bll.Common.xml"));
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        return services;
    }

    private static void AddBusinessLogic(IServiceCollection services)
    {
        services.AddScoped<IBookingManager, BookingManager>();
        services.AddScoped<IConferenceHallManager, ConferenceHallManager>();
        services.AddScoped<IReportManager, ReportManager>();

        services.AddTransient<IPricingManager, PricingManager>();

        services.AddValidatorsFromAssemblyContaining<CreateBookingModelValidator>(
            ServiceLifetime.Transient);

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<
            IDomainEventHandler<BookingCreatedDomainEvent>,
            BookingCreatedEventHandler>();
    }

    private static void AddTelegramNotifications(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TelegramOptions>()
            .Bind(configuration.GetSection(TelegramOptions.SectionName))
            .Validate(options => !options.Enabled ||
                (!string.IsNullOrWhiteSpace(options.BotToken) && !string.IsNullOrWhiteSpace(options.ChatId)),
                "Telegram:BotToken and Telegram:ChatId are required when Telegram is enabled.")
            .ValidateOnStart();
        services.AddHttpClient<TelegramBookingCreatedEventHandler>(client =>
            client.Timeout = TimeSpan.FromSeconds(10))
            // The Telegram request path contains a secret bot token.
            .RemoveAllLoggers();
        services.AddTransient<IDomainEventHandler<BookingCreatedDomainEvent>>(provider =>
            provider.GetRequiredService<TelegramBookingCreatedEventHandler>());
    }

    private static void AddDataAccess(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("The Database connection string is not configured.");

        var commandTimeout = configuration.GetValue("Database:CommandTimeoutSeconds",
            BookingApp.Dal.SqlServerRepositories.Infrastructure.SqlConnectionFactory.DefaultCommandTimeoutSeconds);
        services.AddSqlServerDataAccess(connectionString, commandTimeout);

        services.Configure<DatabaseSeedingOptions>(configuration.GetSection(DatabaseSeedingOptions.SectionName));
        // An optional seeding identity can override the main connection; missing/blank values reuse it.
        services.AddScoped<IDatabaseSeeder>(provider =>
        {
            var seedingConnection = configuration.GetConnectionString("Seeding");
            return new DatabaseSeeder(
                string.IsNullOrWhiteSpace(seedingConnection) ? connectionString : seedingConnection,
                provider.GetRequiredService<ILogger<DatabaseSeeder>>(), commandTimeout);
        });
        services.AddSingleton<StartupDataSeeder>();

        services.Configure<CompleteBookingsOptions>(
            configuration.GetSection(CompleteBookingsOptions.SectionName));
        if (configuration.GetValue("BackgroundJobs:CompleteBookings:Enabled", true))
        {
            services.AddQuartz(CompleteBookingsJobSettings.Configure);
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        }
    }
}
