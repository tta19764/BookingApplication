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
        context.CancellationToken.Returns(token);
        var now = new DateTime(2026, 10, 6, 10, 0, 0, DateTimeKind.Utc);
        clock.GetUtcNow().Returns(new DateTimeOffset(now));
        bookings.CompleteDueAsync(now, 2, token).Returns(2, 1);
        var job = new CompleteBookingsJob(bookings, clock,
            Options.Create(new CompleteBookingsOptions { PageSize = 2 }), Substitute.For<ILogger<CompleteBookingsJob>>());
        await job.Execute(context);
        await bookings.Received(2).CompleteDueAsync(now, 2, token);
        clock.Received(1).GetUtcNow();
    }
}
