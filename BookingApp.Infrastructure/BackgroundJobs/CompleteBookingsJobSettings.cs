using Microsoft.Extensions.Options;
using Quartz;

namespace BookingApp.Infrastructure.BackgroundJobs;

/// <summary>
/// Configures the Quartz schedule for completing expired booking reservations.
/// </summary>
internal class CompleteBookingsJobSettings(IOptions<CompleteBookingsOptions> options) : IConfigureOptions<QuartzOptions>
{
    private readonly CompleteBookingsOptions _options = options.Value;
    private static readonly TriggerKey TriggerKey = new($"{nameof(CompleteBookingsJob)}-trigger");
    
    /// <summary>
    /// Registers the job and its repeating trigger with Quartz.
    /// </summary>
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(CompleteBookingsJob);
        
        // Keep the trigger identity stable so Quartz can update the schedule predictably.
        options.AddJob<CompleteBookingsJob>(jobConfigurator =>
            jobConfigurator.WithIdentity(jobName))
            .AddTrigger(triggerConfigurator =>
                triggerConfigurator
                    .ForJob(jobName)
                    .WithIdentity(TriggerKey)
                    .StartNow()
                    .WithSimpleSchedule(scheduleBuilder =>
                        scheduleBuilder
                            .WithIntervalInSeconds(_options.IntervalSeconds)
                            .RepeatForever()));
    }
}
