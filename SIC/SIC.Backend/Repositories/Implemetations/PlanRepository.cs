using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class PlanRepository : GenericRepository<Plan>, IPlanRepository
{
    private readonly DataContext _context;

    public PlanRepository(DataContext context) : base(context)
    {
        _context = context;
    }
    public async override Task<ActionResponse<IEnumerable<Plan>>> GetAsync()
    {
        var entities = await _context.Plans.Include(p => p.PlanItems).ToListAsync();
        return new ActionResponse<IEnumerable<Plan>>
        {
            Success = true,
            Result = entities
        };
    }
    public async override Task<ActionResponse<Plan>> GetAsync(int id)
    {
        var entities = await _context.Plans.Include(p => p.PlanItems!).ThenInclude(PI => PI.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (entities == null)
        {
            return new ActionResponse<Plan>
            {
                Message = "El registro no existe."
            };
        }
        return new ActionResponse<Plan>
        {
            Success = true,
            Result = entities
        };
    }
}
