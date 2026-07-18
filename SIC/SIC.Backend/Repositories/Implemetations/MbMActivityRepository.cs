using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class MbMActivityRepository : GenericRepository<MbMActivity>, IMbMActivityRepository
{
    private readonly DataContext _context;

    public MbMActivityRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<MbMActivity>> GetWithDetailsAsync(int id)
    {
        var activity = await _context.MbMActivities
            .Include(a => a.Providers)
            .Include(a => a.Tasks)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
        {
            return new ActionResponse<MbMActivity>
            {
                Success = false,
                Message = "La actividad no existe."
            };
        }

        return new ActionResponse<MbMActivity>
        {
            Success = true,
            Result = activity
        };
    }

    public async Task<ActionResponse<IEnumerable<MbMActivity>>> GetByMinuteByMinuteIdAsync(int minuteByMinuteId)
    {
        var activities = await _context.MbMActivities
            .Include(a => a.Providers)
            .Include(a => a.Tasks)
            .Where(a => a.MinuteByMinuteId == minuteByMinuteId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return new ActionResponse<IEnumerable<MbMActivity>>
        {
            Success = true,
            Result = activities
        };
    }
}
