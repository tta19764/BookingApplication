using System.Globalization;
using System.Text.Json;
using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.Shared.Events;
using BookingApp.Bll.Common.Shared.Exceptions;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Users;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace BookingApp.Services.Web.Services.Telegram;

/// <summary>Sends best-effort notifications after a booking has been persisted.</summary>
public sealed class TelegramBookingCreatedEventHandler(
    HttpClient httpClient,
    IOptions<TelegramOptions> options,
    IConferenceHallRepository halls,
    IUserRepository users,
    ILogger<TelegramBookingCreatedEventHandler> logger) : IDomainEventHandler<BookingCreatedDomainEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var settings = options.Value;
        if (!settings.Enabled) return;

        try
        {
            var hall = await halls.GetByIdAsync(domainEvent.ConferenceHallId, cancellationToken);
            var user = await users.GetByIdAsync(domainEvent.UserId, cancellationToken);
            var customerName = user is null ? "Guest" : $"{user.FirstName.Value} {user.LastName.Value}".Trim();
            var text = string.Create(CultureInfo.InvariantCulture, $"""
                New reservation
                Hall: {hall?.Name.Value ?? "Conference hall"}
                Customer: {customerName}
                Start (UTC): {domainEvent.StartTimeUtc:ddd, dd MMM yyyy HH:mm}
                End (UTC): {domainEvent.EndTimeUtc:ddd, dd MMM yyyy HH:mm}
                Duration: {(domainEvent.EndTimeUtc - domainEvent.StartTimeUtc).TotalHours:0.##} hours
                Total: {domainEvent.TotalPrice:0.00} {domainEvent.Currency}
                Status: Reserved
                """);

            // Construct only when enabled so disabled notifications need no bot token.
            var bot = new TelegramBotClient(new TelegramBotClientOptions(settings.BotToken)
            {
                RetryCount = 0
            }, httpClient);
            await bot.SendMessage(settings.ChatId, text,
                messageThreadId: settings.MessageThreadId, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is RequestException or ApiRequestException
            or HttpRequestException or OperationCanceledException or JsonException or PersistenceException)
        {
            // Exception messages and request URLs can contain the bot token. Log only the failure type.
            logger.LogWarning("Telegram notification for booking {BookingId} failed ({FailureType})",
                domainEvent.BookingId, exception.GetType().Name);
        }
    }
}
