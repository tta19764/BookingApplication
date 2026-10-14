using BookingApp.AzureFunctions.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace BookingApp.AzureFunctions.Services;

/// <summary>Delivers report messages through the Telegram Bot API without exposing token-bearing errors.</summary>
/// <param name="httpClient">HTTP transport with request logging disabled.</param>
/// <param name="options">Bot credentials and destination settings.</param>
public sealed class TelegramReportSender(HttpClient httpClient, IOptions<TelegramOptions> options) : ITelegramReportSender
{
    /// <inheritdoc />
    public async Task SendAsync(IReadOnlyList<string> messages, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var bot = new TelegramBotClient(new TelegramBotClientOptions(settings.BotToken) { RetryCount = 0 }, httpClient);
        foreach (var message in messages)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    await bot.SendMessage(settings.ChatId, message, messageThreadId: settings.MessageThreadId,
                        cancellationToken: cancellationToken);
                    break;
                }
                catch (ApiRequestException ex) when (attempt < 2 && ex.ErrorCode == 429 &&
                                                     ex.Parameters?.RetryAfter is > 0 and <= 30)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ex.Parameters.RetryAfter.Value), cancellationToken);
                }
                catch (Exception ex) when (ex is ApiRequestException or HttpRequestException or RequestException
                                            or OperationCanceledException or System.Text.Json.JsonException)
                {
                    if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                    // Do not expose the token-bearing URL through host exception telemetry.
                    throw new InvalidOperationException($"Telegram report delivery failed ({ex.GetType().Name}).");
                }
            }
            if (messages.Count > 1) await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
