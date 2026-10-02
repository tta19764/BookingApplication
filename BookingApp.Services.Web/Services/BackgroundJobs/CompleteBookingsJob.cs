using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
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
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IOptions<CompleteBookingsOptions> options,
    ILogger<CompleteBookingsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var completedCount = 0;
        var pageSize = options.Value.PageSize;

        // Process repeatedly in bounded batches so one run can drain a backlog without loading everything.
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var bookings = await bookingRepository.GetReservedBookingsDueForCompletionAsync(
                utcNow,
                pageSize,
                context.CancellationToken);

            if (bookings.Count == 0)
            {
                break;
            }

            foreach (var booking in bookings)
            {
                if (booking.Status != BookingStatus.Reserved)
                {
                    continue;
                }

                booking.Status = BookingStatus.Completed;
                booking.CompletedOnUtc = utcNow;
                bookingRepository.Update(booking);
                completedCount++;
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            // A short batch means there is no remaining page to fetch for this run.
            if (bookings.Count < pageSize)
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
