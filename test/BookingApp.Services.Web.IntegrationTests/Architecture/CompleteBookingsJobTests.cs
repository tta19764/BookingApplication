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

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class CompleteBookingsJobTests
{
    [Fact]
    public async Task Execute_UsesOneCutoffAndDrainsBoundedBatches()
    {
        var bookings = Substitute.For<IBookingRepository>();
        var clock = Substitute.For<TimeProvider>();
        var context = Substitute.For<IJobExecutionContext>();
        var token = TestContext.Current.CancellationToken;
        var now = new DateTime(2026, 10, 6, 10, 0, 0, DateTimeKind.Utc);
        clock.GetUtcNow().Returns(new DateTimeOffset(now));
        bookings.CompleteDueAsync(now, 2, token).Returns(2, 1);
        var job = new CompleteBookingsJob(bookings, clock,
            Options.Create(new CompleteBookingsOptions { PageSize = 2 }), Substitute.For<ILogger<CompleteBookingsJob>>());
        await job.Execute(context, token);
        await bookings.Received(2).CompleteDueAsync(now, 2, token);
        clock.Received(1).GetUtcNow();
    }

    [Fact]
    public async Task Execute_PreCancelledParameter_DoesNotCallRepository()
    {
        var bookings = Substitute.For<IBookingRepository>();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        IJob job = CreateJob(bookings);
        await job.Execute(Substitute.For<IJobExecutionContext>(), cancellation.Token);
        await bookings.DidNotReceiveWithAnyArgs().CompleteDueAsync(default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_CancelledBetweenBatches_DoesNotStartAnotherBatch()
    {
        var bookings = Substitute.For<IBookingRepository>();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        bookings.CompleteDueAsync(Arg.Any<DateTime>(), 2, cancellation.Token).Returns(_ =>
        {
            cancellation.Cancel();
            return Task.FromResult(2);
        });
        IJob job = CreateJob(bookings);
        await job.Execute(Substitute.For<IJobExecutionContext>(), cancellation.Token);
        await bookings.Received(1).CompleteDueAsync(Arg.Any<DateTime>(), 2, cancellation.Token);
    }

    [Fact]
    public async Task Execute_PersistenceFailure_PropagatesWithoutAnotherBatch()
    {
        var bookings = Substitute.For<IBookingRepository>();
        var token = TestContext.Current.CancellationToken;
        var failure = new PersistenceException(PersistenceError.Timeout, "CompleteDue", Guid.NewGuid());
        bookings.CompleteDueAsync(Arg.Any<DateTime>(), 2, token).Returns(Task.FromException<int>(failure));
        IJob job = CreateJob(bookings);
        Func<Task> execute = async () => await job.Execute(Substitute.For<IJobExecutionContext>(), token);
        (await execute.Should().ThrowAsync<PersistenceException>()).Which.Should().BeSameAs(failure);
        await bookings.Received(1).CompleteDueAsync(Arg.Any<DateTime>(), 2, token);
    }

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

    private static CompleteBookingsJob CreateJob(IBookingRepository bookings) => new(bookings, TimeProvider.System,
        Options.Create(new CompleteBookingsOptions { PageSize = 2 }), Substitute.For<ILogger<CompleteBookingsJob>>());
}
