using BookingApp.Domain.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BookingApp.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for the booking application.
/// </summary>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    /// <summary>
    /// Applies all entity configurations from the infrastructure assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves persistence changes and publishes domain events raised by tracked entities.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // Events are published after the transaction has persisted entity state.
        await PublishDomainEventsAsync();
        
        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        // Copy and clear events before publishing so handlers cannot publish the same event twice.
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entity => entity.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetDomainEvents();
                
                entity.ClearDomainEvents();
                
                return domainEvents;
            })
            .ToList();
        
        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }
}
