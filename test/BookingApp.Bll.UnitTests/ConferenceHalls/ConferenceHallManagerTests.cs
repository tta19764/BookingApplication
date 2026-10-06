using AutoMapper;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Managers.ConferenceHalls;
using BookingApp.Bll.Managers.ConferenceHalls.Validation;
using BookingApp.Bll.Managers.Shared.Validation;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Bll.UnitTests.ConferenceHalls;

public class ConferenceHallManagerTests
{
    [Fact]
    public async Task AddHallAsync_PersistsHall()
    {
        // Arrange
        var repository = Substitute.For<IConferenceHallRepository>();
        var manager = new ConferenceHallManager(repository, Substitute.For<IMapper>(),
            new CreateHallModelValidator(), new UpdateHallModelValidator(), new HallReferenceModelValidator(),
            new FindAvailableHallsModelValidator(), new PaginationModelValidator());
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var model = new CreateHallModel("Hall A", 50, 2000m, "UAH", [Amenity.Projector]);
        Result<Guid> result = await manager.AddHallAsync(model, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Is<ConferenceHall>(hall => hall.Id == result.Value), cancellationToken);
    }
}
