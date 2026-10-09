using AutoMapper;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Errors;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Errors;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Users.Errors;
using BookingApp.Services.Web.Controllers;
using BookingApp.Services.Web.Dtos.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace BookingApp.Services.Web.UnitTests.Controllers;

public sealed class BookingErrorRoutingTests
{
    [Theory]
    [InlineData("hall", 404)]
    [InlineData("user", 404)]
    [InlineData("overlap", 400)]
    [InlineData("unrelated", 400)]
    public async Task CreateBooking_MapsKnownMissingReferencesTo404(string errorKind, int status)
    {
        var error = errorKind switch
        {
            "hall" => ConferenceHallErrors.NotFound,
            "user" => UserErrors.NotFound,
            "overlap" => BookingErrors.Overlap,
            _ => new Error("Other.NotFound", "Unrelated error")
        };
        var manager = Substitute.For<IBookingManager>();
        manager.AddBookingAsync(Arg.Any<CreateBookingModel>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<BookingConfirmationModel>(error));
        var controller = new BookingsController(manager, Substitute.For<IMapper>());
        var request = new CreateBookingRequest(Guid.NewGuid(), new DateOnly(2026, 10, 10),
            new TimeOnly(14, 0), new TimeOnly(15, 0), []);

        var response = await controller.CreateBooking(request, TestContext.Current.CancellationToken);

        response.Should().BeAssignableTo<ObjectResult>().Which.StatusCode.Should().Be(status);
    }
}
