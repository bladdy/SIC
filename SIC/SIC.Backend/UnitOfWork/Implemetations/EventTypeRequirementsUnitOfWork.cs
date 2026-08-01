using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class EventTypeRequirementsUnitOfWork : GenericUnitOfWork<EventTypeRequirement>, IEventTypeRequirementsUnitOfWork
{
    private readonly IEventTypeRequirementsRepository _repository;

    public EventTypeRequirementsUnitOfWork(IGenericRepository<EventTypeRequirement> genericRepository, IEventTypeRequirementsRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<IEnumerable<EventTypeRequirementDTO>>> GetByEventTypeIdAsync(int eventTypeId) => await _repository.GetByEventTypeIdAsync(eventTypeId);
    public async Task<ActionResponse<IEnumerable<EventTypeRequirement>>> GetByEventTypeIdRawAsync(int eventTypeId) => await _repository.GetByEventTypeIdRawAsync(eventTypeId);
    public async Task<ActionResponse<bool>> ExistsAsync(int eventTypeId, int requirementId) => await _repository.ExistsAsync(eventTypeId, requirementId);
}
