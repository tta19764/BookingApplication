using BookingApp.Application.Abstractions.Clock;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace BookingApp.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that completes reserved bookings after their rental period ends.
/// </summary>
[DisallowConcurrentExecution]
internal sealed class CompleteBookingsJob(
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
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
            var utcNow = dateTimeProvider.UtcNow;
            var bookings = await bookingRepository.GetReservedBookingsDueForCompletion(
                utcNow,
                pageSize,
                context.CancellationToken);

            if (bookings.Count == 0)
            {
                break;
            }

            foreach (var booking in bookings)
            {
                // The aggregate owns the status transition and domain event creation.
                var result = booking.Complete(utcNow);

                if (result.IsFailure)
                {
                    continue;
                }

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
