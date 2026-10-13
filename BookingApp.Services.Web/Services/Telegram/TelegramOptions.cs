namespace BookingApp.Services.Web.Services.Telegram;

/// <summary>Configures outbound booking notifications to one Telegram chat.</summary>
public sealed class TelegramOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Telegram";

    /// <summary>Enables notifications; disabled by default.</summary>
    public bool Enabled { get; set; }

    /// <summary>The bot token supplied through secrets or environment configuration.</summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>The destination chat ID or channel username.</summary>
    public string ChatId { get; set; } = string.Empty;

    /// <summary>Optional forum topic ID within the destination chat.</summary>
    public int? MessageThreadId { get; set; }
}
