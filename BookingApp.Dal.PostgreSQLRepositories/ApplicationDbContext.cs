using BookingApp.Bll.Common.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Dal.SqlRepositories;

/// <summary>
/// EF Core database context and unit of work for the booking application.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
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

}
