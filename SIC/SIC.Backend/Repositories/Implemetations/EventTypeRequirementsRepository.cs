using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class EventTypeRequirementsRepository : GenericRepository<EventTypeRequirement>, IEventTypeRequirementsRepository
{
    private readonly DataContext _context;

    public EventTypeRequirementsRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IEnumerable<EventTypeRequirementDTO>>> GetByEventTypeIdAsync(int eventTypeId)
    {
        var entities = await _context.EventTypeRequirements
            .Include(x => x.Requirement)
            .Where(x => x.EventTypeId == eventTypeId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new EventTypeRequirementDTO
            {
                Id = x.Id,
                EventTypeId = x.EventTypeId,
                RequirementId = x.RequirementId,
                SortOrder = x.SortOrder,
                RequirementName = x.Requirement!.Name,
                RequirementSection = x.Requirement.Section,
                RequirementInputType = x.Requirement.InputType,
                RequirementIsRequired = x.Requirement.IsRequired,
                RequirementIsActive = x.Requirement.IsActive,
                RequirementPlaceholder = x.Requirement.Placeholder,
                RequirementMinImages = x.Requirement.MinImages,
                RequirementMaxImages = x.Requirement.MaxImages
            })
            .ToListAsync();

        return new ActionResponse<IEnumerable<EventTypeRequirementDTO>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<IEnumerable<EventTypeRequirement>>> GetByEventTypeIdRawAsync(int eventTypeId)
    {
        var entities = await _context.EventTypeRequirements
            .Include(x => x.Requirement)
            .Where(x => x.EventTypeId == eventTypeId && x.Requirement!.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        return new ActionResponse<IEnumerable<EventTypeRequirement>>
        {
            Success = true,
            Result = entities
        };
    }

    public async Task<ActionResponse<bool>> ExistsAsync(int eventTypeId, int requirementId)
    {
        var exists = await _context.EventTypeRequirements
            .AnyAsync(x => x.EventTypeId == eventTypeId && x.RequirementId == requirementId);

        return new ActionResponse<bool>
        {
            Success = true,
            Result = exists
        };
    }
}
