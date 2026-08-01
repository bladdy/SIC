using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class EventRequirementsRepository : GenericRepository<EventRequirement>, IEventRequirementsRepository
{
    private readonly DataContext _context;

    public EventRequirementsRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IEnumerable<EventRequirement>>> GetBySectionAsync(string section)
    {
        var entities = await _context.EventRequirements
            .Where(x => x.Section == section && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        return new ActionResponse<IEnumerable<EventRequirement>>
        {
            Success = true,
            Result = entities
        };
    }
}
