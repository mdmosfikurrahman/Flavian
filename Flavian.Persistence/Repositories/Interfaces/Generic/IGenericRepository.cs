using Flavian.Shared.Dto;

namespace Flavian.Persistence.Repositories.Interfaces.Generic;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> FindAllAsync(bool includeDeleted = false);
    Task<PaginationResponse<TEntity>> GetAllAsync(int pageNumber, int pageSize);
    Task<TEntity?> FindByIdAsync(Guid id, bool includeDeleted = false);
    Task<TEntity> AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteByIdAsync(Guid id);
    Task<bool> ExistsByIdAsync(Guid id);
    Task<(List<TEntity> Items, int TotalCount)> FindPagedAsync(int pageNumber, int pageSize, bool includeDeleted = false);
    IQueryable<TEntity> GetQueryable();
}
