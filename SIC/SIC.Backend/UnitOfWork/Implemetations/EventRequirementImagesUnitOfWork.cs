using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class EventRequirementImagesUnitOfWork : GenericUnitOfWork<EventRequirementImage>, IEventRequirementImagesUnitOfWork
{
    private readonly IEventRequirementImagesRepository _repository;

    public EventRequirementImagesUnitOfWork(IGenericRepository<EventRequirementImage> genericRepository, IEventRequirementImagesRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<IEnumerable<EventRequirementImage>>> GetByAnswerIdAsync(int answerId) => await _repository.GetByAnswerIdAsync(answerId);
    public async Task<ActionResponse<IEnumerable<EventRequirementImage>>> GetByEventIdAsync(int eventId) => await _repository.GetByEventIdAsync(eventId);
    public async Task<ActionResponse<EventRequirementImage>> GetByIdWithAnswerAsync(int id) => await _repository.GetByIdWithAnswerAsync(id);
}
