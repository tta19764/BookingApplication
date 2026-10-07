using BookingApp.Dal.SqlServerRepositories.Entities;
using FluentAssertions;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class PersistenceEntityTests
{
    [Fact]
    public void SingleKeyPersistenceEntities_InheritMatchingEntityBase()
    {
        Type[] entities = [typeof(BookingEntity), typeof(ConferenceHallEntity), typeof(UserEntity)];
        entities.Should().OnlyContain(type => type.IsSubclassOf(typeof(Entity<Guid>)));

        Type[] catalogs = [typeof(RoleEntity), typeof(PermissionEntity)];
        catalogs.Should().OnlyContain(type => type.IsSubclassOf(typeof(Entity<int>)));
    }

    [Fact]
    public void PersistenceEntities_DoNotExposeBusinessModelsOrProviderTypes()
    {
        var properties = typeof(Entity<>).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(Entity<>).Namespace)
            .SelectMany(type => type.GetProperties());

        foreach (var property in properties)
        {
            var types = property.PropertyType.GetGenericArguments().Append(property.PropertyType);
            types.Should().NotContain(type =>
                (type.Namespace ?? "").StartsWith("BookingApp.Bll")
                || (type.Namespace ?? "").StartsWith("Microsoft.Data.SqlClient"));
        }
    }
}
