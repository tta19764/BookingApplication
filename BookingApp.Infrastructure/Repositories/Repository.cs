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
        return await DbContext.Set<T>()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
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
    
    /// <summary>
    /// Marks an entity as modified in the current persistence context.
    /// </summary>
    public virtual void Update(T entity)
    {
        DbContext.Update(entity);
    }
}
