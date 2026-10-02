using Microsoft.EntityFrameworkCore;
using BookingApp.Bll.Common.Abstractions;
using AutoMapper;

namespace BookingApp.Dal.SqlRepositories.Repositories;

public abstract class Repository<TEntity, TModel>(
    ApplicationDbContext dbContext,
    EntityChangeTracker changeTracker,
    IMapper mapper)
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
        return mapper.Map<IReadOnlyCollection<TModel>>(entities);
    }

    public virtual void Add(TModel model)
    {
        DbSet.Add(mapper.Map<TEntity>(model));
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
        var model = mapper.Map<TModel>(entity);
        changeTracker.Track(() =>
        {
            mapper.Map(model, entity);
        });
        return model;
    }

    protected abstract IQueryable<TEntity> Ordered(IQueryable<TEntity> query);
    protected abstract Guid GetEntityId(TEntity entity);
    protected abstract Guid GetModelId(TModel model);
    protected TModel ToModel(TEntity entity) => mapper.Map<TModel>(entity);
}
