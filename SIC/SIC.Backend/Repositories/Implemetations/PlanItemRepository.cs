using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class PlanItemRepository : GenericRepository<PlanItem>, IPlanItemRepository
{
    private readonly DataContext _context;

    public PlanItemRepository(DataContext context) : base(context)
    {
        _context = context;
    }
    public async override Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync(PaginationDTO pagination)
    {
        var queryable =  _context.PlanItems
            .Include(p => p.Plan)
            .Include(p => p.Item)
            .Where(p => p.PlanId == pagination.Id)
            .AsQueryable();

        return new ActionResponse<IEnumerable<PlanItem>>
        {
            Success = true,
            Result = await queryable.OrderBy(x => x.Item!.Name).Paginate(pagination).ToListAsync()
        };
    }
    public async override Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
    {
        var queryable = _context.PlanItems
            .Where(p => p.PlanId == pagination.Id)
            .AsQueryable();

        double count = await queryable.CountAsync();
        return new ActionResponse<int>
        {
            Success = true,
            Result = (int)count
        };
    }

    public async override Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync()
    {
        var entities = await _context.PlanItems.Include(p => p.Plan).Include(p => p.Item).ToListAsync();
        return new ActionResponse<IEnumerable<PlanItem>>
        {
            Success = true,
            Result = entities
        };
    }
    public async override Task<ActionResponse<PlanItem>> GetAsync(int id)
    {
        var entities = await _context.PlanItems.Include(p => p.Plan).Include(p => p.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (entities == null)
        {
            return new ActionResponse<PlanItem>
            {
                Message = "El registro no existe."
            };
        }
        return new ActionResponse<PlanItem>
        {
            Success = true,
            Result = entities
        };
    }
}
