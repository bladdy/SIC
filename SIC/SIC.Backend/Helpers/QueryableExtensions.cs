using SIC.Shared.DTOs;

namespace SIC.Backend.Helpers;

public static class QueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> querable, PaginationDTO pagination)
    {
        return querable.Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize);
    }
}
