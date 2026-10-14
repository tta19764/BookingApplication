# Hourly booking report

`IBookingReportService` in Common is implemented by the BLL `BookingReportService`.
It returns a `DailyBookingsReport` containing calendar context and booking rows.
Functions owns text formatting and Telegram delivery.
Calendar boundaries are supplied by `IReportingPeriodService` / `ReportingPeriodService`
in Common / BLL. Functions resolves `IBookingsReportFormatter` and
`ITelegramReportSender` for presentation and delivery.

The Functions project reports rentals overlapping today in Europe/Kyiv, including
all statuses. Totals include Reserved and Completed bookings by currency, using
full booking prices, not prorated daily amounts. Empty days produce a message.
Quartz is unchanged. No database initialization happens in the Functions host.

Before enabling reports, apply `006_booking_daily_report.sql` using the existing
DatabaseInitializer setup flow. Existing schema EXECUTE permission covers it;
a separate reporting identity only needs EXECUTE on the new procedure.

Merge `local.settings.example.json` Values into the ignored `local.settings.json`.
These settings are independent of the Web API. Set `KeyVault__Uri`,
`Telegram__ChatId`, and `BookingsReport__Enabled=true`. Optionally set a positive
`Telegram__MessageThreadId` for a topic. Run `az login` for local vault access.
Create secrets `ConnectionStrings--Database` and `Telegram--BotToken` in the vault.
Alternatively omit the vault URI and configure `ConnectionStrings__Database` and
`Telegram__BotToken` locally. The bot needs channel posting permission.

Use Functions Core Tools v4 and Azurite, then run `func start` in the Functions
directory. Hourly schedule: `0 0 * * * *`; testing: `0 */1 * * * *`.
Keep the schedule in host settings: worker Key Vault configuration does not supply
Timer Trigger settings to the host. Outside Development, use a system-assigned
managed identity with Secrets User permission and network access to the vault.
Vault secrets override worker configuration; restart after secret changes.

Messages are plain text, split below 4096 characters. Only explicit rate-limit
responses are retried (twice, up to 30 seconds each). Other failures propagate as
sanitized errors. HTTP client logs are disabled to protect the bot token.
No whole-report retry or durable delivery ledger is provided. A manual retry after
partial delivery can duplicate messages. Late triggers report the current day.
No startup send is configured. Infrastructure and CI/CD are outside this scope.

References: [Timer triggers](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer)
and [Telegram sendMessage](https://core.telegram.org/bots/api#sendmessage).

