using System.Text.Json;
using BookingApp.Services.Web.Dtos.Requests;
using FluentAssertions;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class RequestSerializationTests
{
    [Fact]
    public void CreateBookingRequest_Should_AcceptHourAndMinuteTimeFormat()
    {
        // Arrange
        const string json = """
            {
              "hallId": "11111111-1111-1111-1111-111111111111",
              "date": "2026-10-10",
              "startTime": "10:00",
              "endTime": "11:30",
              "amenities": []
            }
            """;

        // Act
        var request = JsonSerializer.Deserialize<CreateBookingRequest>(json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        // Assert
        request.Should().NotBeNull();
        request.StartTime.Should().Be(new TimeOnly(10, 0));
        request.EndTime.Should().Be(new TimeOnly(11, 30));
    }
}
