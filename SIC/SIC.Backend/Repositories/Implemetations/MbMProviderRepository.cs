using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class MbMProviderRepository : GenericRepository<MbMProvider>, IMbMProviderRepository
{
    private readonly DataContext _context;

    public MbMProviderRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IEnumerable<MbMProvider>>> GetByActivityIdAsync(int activityId)
    {
        var providers = await _context.MbMProviders
            .Where(p => p.MbMActivityId == activityId)
            .ToListAsync();

        return new ActionResponse<IEnumerable<MbMProvider>>
        {
            Success = true,
            Result = providers
        };
    }
}
