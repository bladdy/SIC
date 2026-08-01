using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IEventTypeRequirementsRepository : IGenericRepository<EventTypeRequirement>
{
    Task<ActionResponse<IEnumerable<EventTypeRequirementDTO>>> GetByEventTypeIdAsync(int eventTypeId);
    Task<ActionResponse<IEnumerable<EventTypeRequirement>>> GetByEventTypeIdRawAsync(int eventTypeId);
    Task<ActionResponse<bool>> ExistsAsync(int eventTypeId, int requirementId);
}
