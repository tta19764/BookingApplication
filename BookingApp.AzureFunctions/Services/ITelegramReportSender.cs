namespace BookingApp.AzureFunctions.Services;

/// <summary>Delivers formatted report messages to the configured Telegram destination.</summary>
public interface ITelegramReportSender
{
    /// <summary>Sends message parts in order, retrying bounded rate-limit responses.</summary>
    /// <param name="messages">Plain-text message parts to deliver.</param>
    /// <param name="cancellationToken">Cancels requests and retry delays.</param>
    /// <returns>A task completed after all parts have been delivered.</returns>
    /// <exception cref="InvalidOperationException">Delivery fails; the error is sanitized to protect credentials.</exception>
    Task SendAsync(IReadOnlyList<string> messages, CancellationToken cancellationToken);
}
