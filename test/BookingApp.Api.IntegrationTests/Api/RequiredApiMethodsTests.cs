using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingApp.Api.Contracts;
using BookingApp.Api.Endpoints.ConferenceHalls;
using BookingApp.Application.Bookings.AddBooking;
using BookingApp.Application.ConferenceHalls.AddHall;
using BookingApp.Application.ConferenceHalls.GetHall;
using BookingApp.Api.IntegrationTests.Infrastructure;
using BookingApp.Domain.ConferenceHalls;
using FluentAssertions;

namespace BookingApp.Api.IntegrationTests.Api;

/// <summary>
/// Exercises every HTTP method explicitly required by the assignment.
/// These tests intentionally use HttpClient instead of sending application commands directly,
/// so routing, model binding, serialization, validation, and persistence are covered together.
/// </summary>
public sealed class RequiredApiMethodsTests(IntegrationTestWebAppFactory factory)
    : IClassFixture<IntegrationTestWebAppFactory>
{
    private const string HallsUrl = "/api/v1/conference-halls";
    private const string BookingsUrl = "/api/v1/bookings";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AddConferenceHall_ReturnsCreatedWithUniqueId()
    {
        // Arrange

        // Act
        var hallId = await CreateHallAsync(capacity: 48, hourlyRate: 2100m);

        // Assert
        hallId.Should().NotBeEmpty();

        HallResponse hall = await GetHallAsync(hallId);
        hall.Id.Should().Be(hallId);
        hall.Capacity.Should().Be(48);
        hall.HourlyRate.Should().Be(2100m);
    }

    [Fact]
    public async Task UpdateConferenceHall_PersistsUpdatedInformation()
    {
        // Arrange
        var hallId = await CreateHallAsync(capacity: 40, hourlyRate: 2000m);
        var request = new UpdateConferenceHallRequest(
            $"Updated Hall {Guid.NewGuid():N}",
            55,
            2500m,
            [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]);

        // Act
        using HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"{HallsUrl}/{hallId}",
            request,
            JsonOptions,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HallResponse hall = await GetHallAsync(hallId);
        hall.Name.Should().Be(request.Name);
        hall.Capacity.Should().Be(request.Capacity);
        hall.HourlyRate.Should().Be(request.HourlyRate);
        hall.Amenities.Select(amenity => amenity.Type).Should().BeEquivalentTo(request.Amenities);
    }

    [Fact]
    public async Task DeleteConferenceHall_RemovesHall()
    {
        // Arrange
        var hallId = await CreateHallAsync();

        // Act
        using HttpResponseMessage deleteResponse = await _client.DeleteAsync(
            $"{HallsUrl}/{hallId}",
            TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using HttpResponseMessage getResponse = await _client.GetAsync(
            $"{HallsUrl}/{hallId}",
            TestContext.Current.CancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FindAvailableConferenceHalls_ExcludesAnOverlappingBooking()
    {
        // Arrange
        var bookedHallId = await CreateHallAsync(capacity: 777);
        var availableHallId = await CreateHallAsync(capacity: 777);
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));
        var bookingRequest = new
        {
            HallId = bookedHallId,
            Date = date,
            StartTime = "10:00",
            EndTime = "14:00",
            Amenities = Array.Empty<Amenity>()
        };

        using HttpResponseMessage bookingResponse = await _client.PostAsJsonAsync(
            BookingsUrl,
            bookingRequest,
            JsonOptions,
            TestContext.Current.CancellationToken);
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        string url = $"{HallsUrl}/available?date={date:yyyy-MM-dd}&startTime=10%3A00&endTime=14%3A00&capacity=777";

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            url,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse<List<HallResponse>> body = await ReadAsync<List<HallResponse>>(response);
        body.Data.Should().NotBeNull();
        body.Data.Should().ContainSingle(hall => hall.Id == availableHallId);
        body.Data.Should().NotContain(hall => hall.Id == bookedHallId);
        body.Data.Should().OnlyContain(hall => hall.Capacity >= 777);
    }

    [Fact]
    public async Task BookConferenceHall_ReturnsConfirmationAndCalculatedTotal()
    {
        // Arrange
        var hallId = await CreateHallAsync(hourlyRate: 2000m);
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var request = new
        {
            HallId = hallId,
            Date = date,
            StartTime = "12:00",
            EndTime = "14:00",
            Amenities = new[] { Amenity.Projector, Amenity.WiFi }
        };

        // Act
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            BookingsUrl,
            request,
            JsonOptions,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponse<BookingConfirmationResponse> body =
            await ReadAsync<BookingConfirmationResponse>(response);

        body.Data.Should().NotBeNull();
        body.Data.BookingId.Should().NotBeEmpty();
        body.Data.HallId.Should().Be(hallId);
        body.Data.PriceForPeriod.Should().Be(4600m); // 2 peak hours: 2000 * 1.15 * 2
        body.Data.AmenitiesUpCharge.Should().Be(800m);
        body.Data.TotalPrice.Should().Be(5400m);
        body.Data.Currency.Should().Be("UAH");
    }

    private async Task<Guid> CreateHallAsync(int capacity = 35, decimal hourlyRate = 1900m)
    {
        var command = new AddHallCommand(
            $"API Integration Hall {Guid.NewGuid():N}",
            capacity,
            hourlyRate,
            "UAH",
            [Amenity.Projector, Amenity.WiFi]);

        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            HallsUrl,
            command,
            JsonOptions,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        ApiResponse<Guid> body = await ReadAsync<Guid>(response);
        return body.Data;
    }

    private async Task<HallResponse> GetHallAsync(Guid hallId)
    {
        using HttpResponseMessage response = await _client.GetAsync(
            $"{HallsUrl}/{hallId}",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse<HallResponse> body = await ReadAsync<HallResponse>(response);
        body.Data.Should().NotBeNull();
        return body.Data;
    }

    private static async Task<ApiResponse<T>> ReadAsync<T>(HttpResponseMessage response)
    {
        ApiResponse<T>? body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(
            JsonOptions,
            TestContext.Current.CancellationToken);

        body.Should().NotBeNull();
        return body;
    }
}
