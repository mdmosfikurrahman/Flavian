using Microsoft.EntityFrameworkCore;
using Flavian.Persistence.Data;
using Flavian.Persistence.Helper;
using Flavian.Persistence.Repositories.Interfaces.Generic;
using Flavian.Shared.Dto;

namespace Flavian.Persistence.Repositories.Implementations.Generic;

public class GenericRepository<TEntity>(FlavianDbContext db) : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly FlavianDbContext _db = db;
    private readonly DbSet<TEntity> _dbSet = db.Set<TEntity>();

    public async Task<List<TEntity>> FindAllAsync(bool includeDeleted = false)
    {
        var query = _dbSet.AsQueryable();
        if (!includeDeleted)
            query = query.WhereNotDeleted();

        query = OrderByPrimaryKey(query);

        return await query.ToListAsync();
    }

    public async Task<PaginationResponse<TEntity>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _dbSet.AsQueryable().WhereNotDeleted().AsNoTracking();

        var totalRecords = await query.CountAsync();

        query = OrderByPrimaryKey(query);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResponse<TEntity>
        {
            Data = data,
            TotalRecords = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TEntity?> FindByIdAsync(Guid id, bool includeDeleted = false)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return null;

        if (!includeDeleted &&
            typeof(TEntity).GetProperty("IsDeleted")?.GetValue(entity) is bool and true)
            return null;

        return entity;
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        if (entities is ICollection<TEntity> { Count: 0 })
            return;

        await _dbSet.AddRangeAsync(entities);
    }

    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var entity = await FindByIdAsync(id, includeDeleted: true);
        if (entity != null)
        {
            var isDeletedProp = typeof(TEntity).GetProperty("IsDeleted");
            if (isDeletedProp != null && isDeletedProp.PropertyType == typeof(bool))
            {
                isDeletedProp.SetValue(entity, true);
                _dbSet.Update(entity);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Entity type {typeof(TEntity).Name} does not support soft delete.");
            }
        }
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await FindByIdAsync(id) != null;
    }

    public async Task<(List<TEntity> Items, int TotalCount)> FindPagedAsync(
        int pageNumber, int pageSize, bool includeDeleted = false)
    {
        var query = _dbSet.AsQueryable();
        if (!includeDeleted)
            query = query.WhereNotDeleted();

        var totalCount = await query.CountAsync();

        query = OrderByPrimaryKey(query);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    private IQueryable<TEntity> OrderByPrimaryKey(IQueryable<TEntity> query)
    {
        var entityType = _db.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException($"No entity type metadata for {typeof(TEntity).Name}.");

        var pk = entityType.FindPrimaryKey()
                 ?? throw new InvalidOperationException(
                     $"Entity {typeof(TEntity).Name} has no primary key configured.");

        IOrderedQueryable<TEntity>? ordered = null;
        ordered = query.OrderByDescending(e => EF.Property<DateTime>(e, "CreatedDate"));

        ordered = pk.Properties.Select(prop => prop.Name).Aggregate(ordered,
            (current, name) => current.ThenBy(e => EF.Property<object>(e, name)));

        return ordered ?? query;
    }
}
