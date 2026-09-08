using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class RequirementImageOptionsRepository : GenericRepository<RequirementImageOption>, IRequirementImageOptionsRepository
{
    private readonly DataContext _context;

    public RequirementImageOptionsRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IEnumerable<RequirementImageOption>>> GetByRequirementIdAsync(int requirementId)
    {
        var entities = await _context.RequirementImageOptions
            .Where(x => x.RequirementId == requirementId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        return new ActionResponse<IEnumerable<RequirementImageOption>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<bool>> SaveOptionsAsync(int requirementId, List<RequirementImageOptionDTO> options)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.RequirementImageOptions
                .Where(x => x.RequirementId == requirementId)
                .ToListAsync();

            _context.RequirementImageOptions.RemoveRange(existing);
            await _context.SaveChangesAsync();

            if (options.Count > 0)
            {
                var entities = options.Select(o => new RequirementImageOption
                {
                    RequirementId = requirementId,
                    Title = o.Title,
                    ImageUrl = o.ImageUrl,
                    SortOrder = o.SortOrder
                }).ToList();

                await _context.RequirementImageOptions.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ActionResponse<bool>
            {
                Message = ex.Message
            };
        }
    }
}
