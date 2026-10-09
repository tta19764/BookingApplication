using BookingApp.Services.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.ArchitectureTests;

[Trait("Category", "Architecture")]
public sealed class ControllerArchitectureTests
{
    [Fact]
    public void ApiControllers_Should_UseMvcAndNotDependOnRepositories()
    {
        // Arrange
        var controllerTypes = typeof(BookingsController).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .ToList();

        // Act
        var repositoryDependencies = controllerTypes
            .SelectMany(type => type.GetConstructors())
            .SelectMany(constructor => constructor.GetParameters())
            .Where(parameter => parameter.ParameterType.Name.EndsWith("Repository", StringComparison.Ordinal))
            .ToList();

        // Assert
        controllerTypes.Should().HaveCount(3);
        repositoryDependencies.Should().BeEmpty();
    }
}
