﻿using BookingApp.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Infrastructure.Repositories;

/// <summary>
/// Base EF Core repository for aggregate roots with a Guid identifier.
/// </summary>
public abstract class Repository<T>(ApplicationDbContext dbContext)
    where T : Entity
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();

    /// <summary>
    /// Finds an entity by identifier.
    /// </summary>
    public async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    /// <summary>
    /// Returns a deterministic page of entities ordered by identifier.
    /// </summary>
    public async Task<IReadOnlyCollection<T>> GetListPaginatedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
        }

        return await DbSet
            .AsNoTracking()
            .OrderBy(entity => entity.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds an entity to the current persistence context.
    /// </summary>
    public virtual void Add(T entity)
    {
        DbContext.Add(entity);
    }
    
    /// <summary>
    /// Removes an entity from the current persistence context.
    /// </summary>
    public virtual void Remove(T entity)
    {
        DbContext.Remove(entity);
    }
}
