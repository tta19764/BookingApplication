using BookingApp.Bll.Common.Shared;
using BookingApp.Dal.SqlRepositories.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Dal.SqlRepositories;

/// <summary>
/// EF Core database context and unit of work for the booking application.
/// </summary>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    EntityChangeTracker entityChangeTracker)
    : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Applies all entity configurations from the infrastructure assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Synchronizes tracked business models and saves persistence changes.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        entityChangeTracker.Apply();
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
}
