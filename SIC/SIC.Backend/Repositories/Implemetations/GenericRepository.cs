using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Response;
using System;

namespace SIC.Backend.Repositories.Implemetations;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DataContext _context;
    private readonly DbSet<T> _entity;

    public GenericRepository(DataContext context)
    {
        _context = context;
        _entity = _context.Set<T>();
    }

    public virtual async Task<ActionResponse<IEnumerable<T>>> GetAsync(PaginationDTO pagination)
    {
        var queryable = _entity.AsQueryable();
        return new ActionResponse<IEnumerable<T>>
        {
            Success = true,
            Result = await queryable.Paginate(pagination).ToListAsync()
        };
    }

    public virtual async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
    {
        var queryable = _entity.AsQueryable();
        double count = await queryable.CountAsync();
        int totalPages = (int)Math.Ceiling((double)count / pagination.PageSize);
        return new ActionResponse<int> {
            Success = true,
            Result = totalPages
        };
    }

    public virtual async Task<ActionResponse<T>> AddAsync(T entity)
    {
        _context.Add(entity);
        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<T>
            {
                Success = true,
                Result = entity
            };
        }
        catch (DbUpdateException)
        {
            return DbUpdateExceptionActionResponse();
        }
        catch (Exception exception)
        {
            return ExceptionActionResponse(exception);
        }
    }

    public virtual async Task<ActionResponse<T>> DeleteAsync(int id)
    {
        var entity = await _entity.FindAsync(id);
        if (entity == null)
        {
            return new ActionResponse<T>
            {
                Message = "El registro no existe."
            };
        }
        _entity.Remove(entity);
        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<T>
            {
                Success = true
            };
        }
        catch (Exception)
        {
            return new ActionResponse<T>
            {
                Message = "No se puede eliminar, porque tiene registros relacionados."
            };
        }

    }

    public virtual async Task<ActionResponse<T>> GetAsync(int id)
    {
        var entity = await _entity.FindAsync(id);
        if (entity == null)
        {
            return new ActionResponse<T>
            {
                Message = "El registro no existe."
            };
        }
        return new ActionResponse<T>
        {
            Success = true,
            Result = entity
        };
    }

    public virtual async Task<ActionResponse<IEnumerable<T>>> GetAsync() => new
    ActionResponse<IEnumerable<T>>
    {
        Success = true,
        Result = await _entity.ToListAsync()
    };

    public virtual async Task<ActionResponse<T>> UpdateAsync(T entity)
    {
        _context.Update(entity);
        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<T>
            {
                Success = true,
                Result = entity
            };
        }
        catch (DbUpdateException)
        {
            return DbUpdateExceptionActionResponse();
        }
        catch (Exception exception)
        {
            return ExceptionActionResponse(exception);
        }
    }
    

    private ActionResponse<T> ExceptionActionResponse(Exception exception) => new()
    {
        Message = exception.Message
    };

    private ActionResponse<T> DbUpdateExceptionActionResponse() => new()
    {
        Message = "El registro ya existe."
    };

}