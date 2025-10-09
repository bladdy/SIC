using SIC.Shared.DTOs;
using SIC.Shared.Response;
using System.Linq.Expressions;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IGenericUnitOfWork<T> where T : class
{
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    Task<ActionResponse<IEnumerable<T>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);

    Task<ActionResponse<T>> GetAsync(int id);

    Task<ActionResponse<IEnumerable<T>>> GetAsync();

    Task<ActionResponse<T>> AddAsync(T entity);

    Task<ActionResponse<T>> DeleteAsync(int id);

    Task<ActionResponse<T>> UpdateAsync(T entity);
}