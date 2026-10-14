using BookingApp.AzureFunctions.Configuration;
using BookingApp.AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingApp.AzureFunctions.Functions;

/// <summary>Coordinates daily report retrieval, formatting, and Telegram delivery on a timer.</summary>
/// <param name="logger">Logger for timer and delivery status.</param>
/// <param name="options">Report enablement and calendar settings.</param>
/// <param name="services">Invocation services, resolved only when reporting is enabled.</param>
public class BookingsReportSchedule(ILogger<BookingsReportSchedule> logger,
    IOptions<ReportOptions> options, IServiceProvider services)
{
    /// <summary>Sends the current day's report when scheduled reporting is enabled.</summary>
    /// <param name="timer">Timer invocation metadata, including missed schedule information.</param>
    /// <param name="cancellationToken">Cancels database queries and Telegram delivery.</param>
    /// <returns>A task completed after delivery or an early return when disabled.</returns>
    [Function("BookingsReportSchedule")]
    public async Task Run([TimerTrigger("%BookingsReportSchedule%", RunOnStartup = false)] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled) { logger.LogInformation("Bookings reporting is disabled."); return; }
        if (timer.IsPastDue) logger.LogWarning("Bookings report timer is past due; reporting the current day.");
        var report = await services.GetRequiredService<BookingApp.Bll.Common.Bookings.IBookingReportService>()
            .GetTodayAsync(options.Value.TimeZoneId, cancellationToken);
        var messages = services.GetRequiredService<IBookingsReportFormatter>().Format(report.Date,
            TimeZoneInfo.FindSystemTimeZoneById(report.TimeZoneId), report.Bookings);
        await services.GetRequiredService<ITelegramReportSender>().SendAsync(messages, cancellationToken);
        logger.LogInformation("Bookings report sent in {MessageCount} message(s).", messages.Count);
    }
}
