namespace BookingApp.AzureFunctions.Configuration;

/// <summary>Controls report delivery and calendar interpretation.</summary>
public sealed class ReportOptions
{
    /// <summary>Gets or sets whether scheduled report delivery is enabled; false by default.</summary>
    public bool Enabled { get; set; }
    /// <summary>Gets or sets the calendar time zone; defaults to Europe/Kyiv.</summary>
    public string TimeZoneId { get; set; } = "Europe/Kyiv";
}

/// <summary>Telegram destination and credentials, independent of the Web API settings.</summary>
public sealed class TelegramOptions
{
    /// <summary>Gets or sets the secret bot token used to authorize Telegram requests.</summary>
    public string BotToken { get; set; } = "";
    /// <summary>Gets or sets the destination chat or channel identifier.</summary>
    public string ChatId { get; set; } = "";
    /// <summary>Gets or sets an optional positive forum topic identifier.</summary>
    public int? MessageThreadId { get; set; }
}
