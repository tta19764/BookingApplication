using Microsoft.EntityFrameworkCore;
using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Dal.SqlRepositories.Repositories;

public abstract class Repository<TEntity, TModel>(
    ApplicationDbContext dbContext,
    EntityChangeTracker changeTracker)
    where TEntity : class
    where TModel : Entity
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    public async Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync([id], cancellationToken);
        return entity is null ? default : Track(entity);
    }

    public async Task<IReadOnlyCollection<TModel>> GetListPaginatedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var entities = await Ordered(DbSet.AsNoTracking())
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return entities.Select(ToModel).ToList();
    }

    public virtual void Add(TModel model)
    {
        changeTracker.TrackEvents(model);
        DbSet.Add(ToEntity(model));
    }

    public virtual void Remove(TModel model)
    {
        var id = GetModelId(model);
        var entity = DbSet.Local.FirstOrDefault(item => GetEntityId(item) == id)
            ?? throw new InvalidOperationException("The entity must be loaded before it can be removed.");
        DbSet.Remove(entity);
    }

    protected TModel Track(TEntity entity)
    {
        var model = ToModel(entity);
        changeTracker.Track(() =>
        {
            changeTracker.TrackEvents(model);
            UpdateEntity(entity, model);
        });
        return model;
    }

    protected abstract IQueryable<TEntity> Ordered(IQueryable<TEntity> query);
    protected abstract Guid GetEntityId(TEntity entity);
    protected abstract Guid GetModelId(TModel model);
    protected abstract TEntity ToEntity(TModel model);
    protected abstract TModel ToModel(TEntity entity);
    protected abstract void UpdateEntity(TEntity entity, TModel model);
}
