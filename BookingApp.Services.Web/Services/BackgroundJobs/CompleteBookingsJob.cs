using BookingApp.Bll.Common.Bookings;
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
    /// <summary>Completes expired reservations in bounded batches using one UTC cutoff.</summary>
    /// <param name="context">Quartz execution context.</param>
    /// <param name="cancellationToken">Quartz interruption or shutdown token, forwarded to every database operation.</param>
    /// <returns>A task completed after all due batches are processed or cancellation stops the loop.</returns>
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var completedCount = 0;
        var pageSize = options.Value.PageSize;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        // Process repeatedly in bounded batches so one run can drain a backlog without loading everything.
        while (!cancellationToken.IsCancellationRequested)
        {
            var count = await bookingRepository.CompleteDueAsync(
                utcNow,
                pageSize,
                cancellationToken);

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
