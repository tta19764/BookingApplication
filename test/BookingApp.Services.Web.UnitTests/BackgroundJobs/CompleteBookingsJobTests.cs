using BookingApp.Bll.Common.Shared.Exceptions;
using FluentAssertions;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Services.Web.Services.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Quartz;

namespace BookingApp.Services.Web.UnitTests.BackgroundJobs;

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

    private static CompleteBookingsJob CreateJob(IBookingRepository bookings) => new(bookings, TimeProvider.System,
        Options.Create(new CompleteBookingsOptions { PageSize = 2 }), Substitute.For<ILogger<CompleteBookingsJob>>());
}
