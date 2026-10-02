using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BookingApp.Dal.SqlRepositories.Entities;

namespace BookingApp.Dal.SqlRepositories.Repositories;

public abstract class Repository<TEntity, TModel>(
    ApplicationDbContext dbContext,
    IMapper mapper)
    where TEntity : Entity
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    public async Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync([id], cancellationToken);
        return entity is null ? default : mapper.Map<TModel>(entity);
    }

    public async Task<IReadOnlyCollection<TModel>> GetListPaginatedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var entities = await DbSet
            .AsNoTracking()
            .OrderBy(entity => entity.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyCollection<TModel>>(entities);
    }

    public virtual void Add(TModel model)
    {
        DbSet.Add(mapper.Map<TEntity>(model));
    }

    protected void UpdateEntity(Guid id, TModel model)
    {
        var entity = DbSet.Find(id)
            ?? throw new InvalidOperationException($"{typeof(TEntity).Name} with ID '{id}' was not found.");

        mapper.Map(model, entity);
    }

    protected void RemoveEntity(Guid id)
    {
        var entity = DbSet.Find(id)
            ?? throw new InvalidOperationException($"{typeof(TEntity).Name} with ID '{id}' was not found.");

        DbSet.Remove(entity);
    }

    protected TModel ToModel(TEntity entity) => mapper.Map<TModel>(entity);
}
