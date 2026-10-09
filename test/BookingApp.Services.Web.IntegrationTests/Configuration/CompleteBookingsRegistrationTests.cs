using BookingApp.Bll.Common.Shared.Exceptions;
using BookingApp.Services.Web.DI;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Services.Web.Services.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Quartz;

namespace BookingApp.Services.Web.IntegrationTests.Configuration;

public sealed class CompleteBookingsRegistrationTests
{
    [Fact]
    public async Task Registration_ConfiguresQuartz4JobAndIntervalFromOptions()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddApi(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=localhost;Database=Booking;Integrated Security=true",
            ["DatabaseSeeding:Enabled"] = "false",
            ["BackgroundJobs:CompleteBookings:Enabled"] = "true",
            ["BackgroundJobs:CompleteBookings:IntervalSeconds"] = "17",
            ["BackgroundJobs:CompleteBookings:PageSize"] = "2"
        }).Build());
        await using var provider = services.BuildServiceProvider();
        var token = TestContext.Current.CancellationToken;
        var scheduler = await provider.GetRequiredService<ISchedulerFactory>().GetScheduler(token);
        try
        {
            var jobKey = new JobKey(nameof(CompleteBookingsJob));
            var job = await scheduler.GetJobDetail(jobKey, token);
            job.Should().NotBeNull();
            job!.JobType.Type.Should().Be(typeof(CompleteBookingsJob));
            job.ConcurrentExecutionDisallowed.Should().BeTrue();
            var trigger = await scheduler.GetTrigger(new TriggerKey($"{nameof(CompleteBookingsJob)}-trigger"), token);
            var simple = trigger.Should().BeAssignableTo<ISimpleTrigger>().Subject;
            simple.JobKey.Should().Be(jobKey);
            simple.RepeatInterval.Should().Be(TimeSpan.FromSeconds(17));
            simple.RepeatCount.Should().Be(-1);
        }
        finally
        {
            await scheduler.Shutdown(cancellationToken: CancellationToken.None);
        }
    }

}
