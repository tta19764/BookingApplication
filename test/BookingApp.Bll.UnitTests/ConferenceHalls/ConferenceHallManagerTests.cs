using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.ConferenceHalls;
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
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var manager = new ConferenceHallManager(repository, unitOfWork);
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        Result<Guid> result = await manager.AddHallAsync("Hall A", 50, 2000m, "UAH",
            [Amenity.Projector], cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        repository.Received(1).Add(Arg.Is<ConferenceHall>(hall => hall.Id == result.Value));
        await unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }
}
