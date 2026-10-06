using BookingApp.Dal.SqlRepositories.Entities;
using FluentAssertions;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class PersistenceEntityTests
{
    [Fact]
    public void PersistenceEntities_InheritEntityBase()
    {
        // Arrange
        Type entityBase = typeof(Entity);
        Type[] persistenceEntities = [typeof(BookingEntity), typeof(ConferenceHallEntity), typeof(UserEntity)];

        // Act
        var invalidEntities = persistenceEntities.Where(type => !type.IsSubclassOf(entityBase)).ToList();

        // Assert
        invalidEntities.Should().BeEmpty();
    }
}
