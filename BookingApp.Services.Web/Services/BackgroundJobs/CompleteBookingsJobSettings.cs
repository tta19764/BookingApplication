using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace BookingApp.Services.Web.Services.BackgroundJobs;

/// <summary>Configures the Quartz 4 job and trigger through its registration builder.</summary>
public static class CompleteBookingsJobSettings
{
    /// <summary>Registers a stable job identity and repeating trigger using configured options.</summary>
    /// <param name="builder">Quartz registration builder.</param>
    public static void Configure(IQuartzBuilder builder)
    {
        const string jobName = nameof(CompleteBookingsJob);
        builder.AddJob<CompleteBookingsJob>(job => job.WithIdentity(jobName));
        builder.AddTrigger((provider, trigger) =>
        {
            var options = provider.GetRequiredService<IOptions<CompleteBookingsOptions>>().Value;
            trigger.ForJob(jobName)
                .WithIdentity(new TriggerKey($"{jobName}-trigger"))
                .StartNow()
                .WithSimpleSchedule(schedule => schedule
                    .WithInterval(TimeSpan.FromSeconds(options.IntervalSeconds))
                    .RepeatForever());
        });
    }
}
