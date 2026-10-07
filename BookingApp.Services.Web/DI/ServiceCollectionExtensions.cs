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

        services.AddSwaggerGen();
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

    private static void AddDataAccess(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("The Database connection string is not configured.");

        var commandTimeout = configuration.GetValue("Database:CommandTimeoutSeconds", 30);
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
            services.AddQuartz();
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
            services.ConfigureOptions<CompleteBookingsJobSettings>();
        }
    }
}
