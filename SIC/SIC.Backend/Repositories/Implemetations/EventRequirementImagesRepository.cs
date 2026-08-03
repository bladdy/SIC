using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class EventRequirementImagesRepository : GenericRepository<EventRequirementImage>, IEventRequirementImagesRepository
{
    private readonly DataContext _context;

    public EventRequirementImagesRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IEnumerable<EventRequirementImage>>> GetByAnswerIdAsync(int answerId)
    {
        var entities = await _context.EventRequirementImages
            .Where(x => x.RequirementAnswerId == answerId)
            .OrderBy(x => x.Order)
            .ToListAsync();

        return new ActionResponse<IEnumerable<EventRequirementImage>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<IEnumerable<EventRequirementImage>>> GetByEventIdAsync(int eventId)
    {
        var entities = await _context.EventRequirementImages
            .Include(x => x.RequirementAnswer)
            .Where(x => x.RequirementAnswer!.EventId == eventId)
            .OrderBy(x => x.Order)
            .ToListAsync();

        return new ActionResponse<IEnumerable<EventRequirementImage>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<EventRequirementImage>> GetByIdWithAnswerAsync(int id)
    {
        var entity = await _context.EventRequirementImages
            .Include(x => x.RequirementAnswer)
            .FirstOrDefaultAsync(x => x.Id == id);

        return new ActionResponse<EventRequirementImage>
        {
            Success = true,
            Result = entity
        };
    }
}
