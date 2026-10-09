using BookingApp.Bll.Common.Shared.Events;
using BookingApp.Services.Web.Services.DomainEvents;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Services.Web.UnitTests.Services;

public sealed class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_InvokesRegisteredHandler()
    {
        // Arrange
        var handler = new TestDomainEventHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<TestDomainEvent>>(handler);
        await using var provider = services.BuildServiceProvider();
        var dispatcher = new DomainEventDispatcher(provider);
        var domainEvent = new TestDomainEvent(DateTime.UtcNow);

        // Act
        await dispatcher.DispatchAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        handler.HandledEvent.Should().BeSameAs(domainEvent);
    }

    private sealed record TestDomainEvent(DateTime OccurredOnUtc) : IDomainEvent;

    private sealed class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public TestDomainEvent? HandledEvent { get; private set; }

        public Task HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            HandledEvent = domainEvent;
            return Task.CompletedTask;
        }
    }
}
