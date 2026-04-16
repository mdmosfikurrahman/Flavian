using Flavian.Shared.Dto;

namespace Flavian.Application.Services.Interfaces.Generic;

public interface IGenericService<T> where T : class
{
    Task<StandardResponse> GetAllAsync(int? pageNumber, int? pageSize);
    Task<StandardResponse> GetByIdAsync(Guid id);
    Task<StandardResponse> AddAsync(T entity);
    Task<StandardResponse> UpdateAsync(Guid id, T entity);
    Task<StandardResponse> DeleteAsync(Guid id);
}
