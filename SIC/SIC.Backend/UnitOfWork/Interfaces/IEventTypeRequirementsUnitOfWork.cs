using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IEventTypeRequirementsUnitOfWork : IGenericUnitOfWork<EventTypeRequirement>
{
    Task<ActionResponse<IEnumerable<EventTypeRequirementDTO>>> GetByEventTypeIdAsync(int eventTypeId);
    Task<ActionResponse<IEnumerable<EventTypeRequirement>>> GetByEventTypeIdRawAsync(int eventTypeId);
    Task<ActionResponse<bool>> ExistsAsync(int eventTypeId, int requirementId);
}
