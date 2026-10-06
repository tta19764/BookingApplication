using BookingApp.Bll.Common.Bookings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace BookingApp.Services.Web.Services.BackgroundJobs;

/// <summary>
/// Background job that completes reserved bookings after their rental period ends.
/// </summary>
[DisallowConcurrentExecution]
public sealed class CompleteBookingsJob(
    IBookingRepository bookingRepository,
    TimeProvider timeProvider,
    IOptions<CompleteBookingsOptions> options,
    ILogger<CompleteBookingsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var completedCount = 0;
        var pageSize = options.Value.PageSize;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        // Process repeatedly in bounded batches so one run can drain a backlog without loading everything.
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var count = await bookingRepository.CompleteDueAsync(
                utcNow,
                pageSize,
                context.CancellationToken);

            if (count == 0)
            {
                break;
            }

            completedCount += count;

            // A short batch means there is no remaining page to fetch for this run.
            if (count < pageSize)
            {
                break;
            }
        }

        if (completedCount > 0)
        {
            logger.LogInformation(
                "Completed {CompletedBookingsCount} expired booking reservations",
                completedCount);
        }
    }
}
