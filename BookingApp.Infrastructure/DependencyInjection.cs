using System.Text.Json.Serialization;
using Asp.Versioning;
using BookingApp.Application.Abstractions.Clock;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Users;
using BookingApp.Infrastructure.BackgroundJobs;
using BookingApp.Infrastructure.Clock;
using BookingApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace BookingApp.Infrastructure;

/// <summary>
/// Registers infrastructure services such as persistence, repositories, clock, and background jobs.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the infrastructure layer to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        AddPersistence(services, configuration);
        
        AddBackgroundJobs(services, configuration);

        return services;
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        // Quartz reads this options object both when scheduling the job and when the job executes.
        services.Configure<CompleteBookingsOptions>(configuration.GetSection(CompleteBookingsOptions.SectionName));
        
        services.AddQuartz();

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        
        services.ConfigureOptions<CompleteBookingsJobSettings>();
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Database") ??
            throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IConferenceHallRepository, ConferenceHallRepository>();

        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
    }
    
    public static void AddApiVersioning(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
    }}
