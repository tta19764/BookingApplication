using AutoMapper;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Errors;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Services.Web.Controllers;
using BookingApp.Services.Web.Dtos.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace BookingApp.Services.Web.UnitTests.Controllers;

public sealed class ConferenceHallErrorRoutingTests
{
    [Theory]
    [InlineData(true, 404, 404)]
    [InlineData(false, 400, 409)]
    public async Task Mutations_Return404OnlyForConferenceHallNotFound(
        bool hallMissing, int updateStatus, int deleteStatus)
    {
        var manager = Substitute.For<IConferenceHallManager>();
        var error = hallMissing
            ? ConferenceHallErrors.NotFound
            : new Error("Other.NotFound", "Unrelated error");
        manager.UpdateHallAsync(Arg.Any<UpdateHallModel>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));
        manager.RemoveHallAsync(Arg.Any<HallReferenceModel>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));
        var controller = new ConferenceHallsController(manager, Substitute.For<IMapper>());
        var token = TestContext.Current.CancellationToken;

        var update = await controller.UpdateConferenceHall(Guid.NewGuid(),
            new UpdateConferenceHallRequest("Hall", 10, 100m, []), token);
        var delete = await controller.DeleteConferenceHall(Guid.NewGuid(), token);

        update.Should().BeAssignableTo<ObjectResult>().Which.StatusCode.Should().Be(updateStatus);
        delete.Should().BeAssignableTo<ObjectResult>().Which.StatusCode.Should().Be(deleteStatus);
    }
}
