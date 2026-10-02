using Asp.Versioning;
using BookingApp.Services.Web.Configuration;
using BookingApp.Services.Web.Mappings;
using System.Text.Json.Serialization;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Shared.Events;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Reports;
using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Managers.ConferenceHalls;
using BookingApp.Bll.Managers.Reports;
using BookingApp.Bll.Managers.Bookings.Validation;
using BookingApp.Bll.Managers.ConferenceHalls.Validation;
using BookingApp.Bll.Managers.Shared.Validation;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared.Models;
using BookingApp.Dal.SqlRepositories;
using BookingApp.Dal.SqlRepositories.Repositories;
using BookingApp.Services.Web.Services.BackgroundJobs;
using BookingApp.Services.Web.Services.DomainEvents;
using Microsoft.EntityFrameworkCore;
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
            typeof(BookingApp.Dal.SqlRepositories.Mappings.AutoMapperConfig));

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
        services.AddTransient<IValidator<PaginationModel>, PaginationModelValidator>();
        services.AddTransient<IValidator<CreateBookingModel>, CreateBookingModelValidator>();
        services.AddTransient<IValidator<CreateHallModel>, CreateHallModelValidator>();
        services.AddTransient<IValidator<UpdateHallModel>, UpdateHallModelValidator>();
        services.AddTransient<IValidator<HallReferenceModel>, HallReferenceModelValidator>();
        services.AddTransient<IValidator<FindAvailableHallsModel>, FindAvailableHallsModelValidator>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<BookingApp.Bll.Common.Bookings.Events.BookingCreatedDomainEvent>,
            BookingCreatedEventHandler>();
    }

    private static void AddDataAccess(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("The Database connection string is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConferenceHallRepository, ConferenceHallRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<CompleteBookingsOptions>(
            configuration.GetSection(CompleteBookingsOptions.SectionName));
        services.AddQuartz();
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        services.ConfigureOptions<CompleteBookingsJobSettings>();
    }
}
