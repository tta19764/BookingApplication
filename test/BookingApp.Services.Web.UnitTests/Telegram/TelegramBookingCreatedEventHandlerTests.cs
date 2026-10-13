using System.Net;
using System.Text.Json;
using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Services.Web.Services.Telegram;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace BookingApp.Services.Web.UnitTests.Telegram;

public sealed class TelegramBookingCreatedEventHandlerTests
{
    private static readonly BookingCreatedDomainEvent Booking = new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 123.45m, "USD",
        new DateTime(2026, 10, 13, 8, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 10, 14, 9, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 10, 14, 11, 0, 0, DateTimeKind.Utc));

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    public async Task Enabled_SendsBookingDetailsToConfiguredChat(int? messageThreadId)
    {
        using var transport = new Transport();
        using var client = new HttpClient(transport);
        var logger = Substitute.For<ILogger<TelegramBookingCreatedEventHandler>>();
        var handler = Create(client, logger: logger, messageThreadId: messageThreadId);
        await handler.HandleAsync(Booking, TestContext.Current.CancellationToken);

        transport.Method.Should().Be(HttpMethod.Post);
        transport.Uri.Should().Be("https://api.telegram.org/bot123:test-token/sendMessage");
        using var payload = JsonDocument.Parse(transport.Payload!);
        payload.RootElement.GetProperty("chat_id").ToString().Should().Be("-100123");
        if (messageThreadId.HasValue)
            payload.RootElement.GetProperty("message_thread_id").GetInt32().Should().Be(messageThreadId.Value);
        else
            payload.RootElement.TryGetProperty("message_thread_id", out _).Should().BeFalse();
        var text = payload.RootElement.GetProperty("text").GetString();
        text.Should().Contain("Hall: Grand Hall").And.Contain("Customer: Alex Smith").And.Contain("123.45 USD")
            .And.Contain("Start (UTC): Wed, 14 Oct 2026 09:00").And.Contain("End (UTC): Wed, 14 Oct 2026 11:00")
            .And.Contain("Duration: 2 hours").And.Contain("Status: Reserved")
            .And.NotContain(Booking.BookingId.ToString()).And.NotContain(Booking.ConferenceHallId.ToString())
            .And.NotContain(Booking.UserId.ToString());
        transport.Calls.Should().Be(1);
        logger.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Disabled_DoesNotSend()
    {
        using var transport = new Transport();
        using var client = new HttpClient(transport);
        await Create(client, false).HandleAsync(Booking, TestContext.Current.CancellationToken);
        transport.Calls.Should().Be(0);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{\"ok\":false,\"error_code\":400,\"description\":\"Bad Request\"}")]
    [InlineData(HttpStatusCode.TooManyRequests, "{\"ok\":false,\"error_code\":429,\"description\":\"Too Many Requests\",\"parameters\":{\"retry_after\":1}}")]
    [InlineData(HttpStatusCode.OK, "{\"ok\":false}")]
    [InlineData(HttpStatusCode.OK, "invalid json")]
    public async Task ApiFailure_DoesNotFailPersistedBooking(HttpStatusCode status, string body)
    {
        using var transport = new Transport { Status = status, Body = body };
        using var client = new HttpClient(transport);
        await Create(client).HandleAsync(Booking, TestContext.Current.CancellationToken);
        transport.Calls.Should().Be(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NetworkFailureOrTimeout_DoesNotFailPersistedBooking(bool timeout)
    {
        using var transport = new Transport
        {
            Failure = timeout ? new TaskCanceledException() : new HttpRequestException("secret-url")
        };
        using var client = new HttpClient(transport);
        await Create(client).HandleAsync(Booking, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Cancelled_DoesNotSend()
    {
        using var transport = new Transport();
        using var client = new HttpClient(transport);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> act = () => Create(client).HandleAsync(Booking, cancellation.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
        transport.Calls.Should().Be(0);
    }

    private static TelegramBookingCreatedEventHandler Create(HttpClient client, bool enabled = true,
        ILogger<TelegramBookingCreatedEventHandler>? logger = null, int? messageThreadId = null)
    {
        var halls = Substitute.For<IConferenceHallRepository>();
        var users = Substitute.For<IUserRepository>();
        halls.GetByIdAsync(Booking.ConferenceHallId, Arg.Any<CancellationToken>())
            .Returns(new ConferenceHall { Name = new Name("Grand Hall") });
        users.GetByIdAsync(Booking.UserId, Arg.Any<CancellationToken>())
            .Returns(new User { FirstName = new FirstName("Alex"), LastName = new LastName("Smith") });
        return new(client, Options.Create(new TelegramOptions
        {
            Enabled = enabled, BotToken = "123:test-token", ChatId = "-100123", MessageThreadId = messageThreadId
        }), halls, users, logger ?? Substitute.For<ILogger<TelegramBookingCreatedEventHandler>>());
    }

    private sealed class Transport : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string? Uri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Payload { get; private set; }
        public HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
        public string Body { get; init; } = """
            {"ok":true,"result":{"message_id":1,"date":1791878400,"chat":{"id":-100123,"type":"supergroup","title":"Bookings"},"text":"New booking created"}}
            """;
        public Exception? Failure { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Uri = request.RequestUri!.ToString();
            Method = request.Method;
            Payload = await request.Content!.ReadAsStringAsync(cancellationToken);
            if (Failure is not null) throw Failure;
            return new HttpResponseMessage(Status) { Content = new StringContent(Body) };
        }
    }
}
