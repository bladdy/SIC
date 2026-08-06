using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class EventRequirementAnswersUnitOfWork : GenericUnitOfWork<EventRequirementAnswer>, IEventRequirementAnswersUnitOfWork
{
    private readonly IEventRequirementAnswersRepository _repository;

    public EventRequirementAnswersUnitOfWork(IGenericRepository<EventRequirementAnswer> genericRepository, IEventRequirementAnswersRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<IEnumerable<EventRequirementAnswer>>> GetByEventIdAsync(int eventId) => await _repository.GetByEventIdAsync(eventId);
    public async Task<ActionResponse<bool>> SaveAllAsync(int eventId, List<EventRequirementAnswerDTO> answers) => await _repository.SaveAllAsync(eventId, answers);
    public async Task<ActionResponse<SaveFormResponseDTO>> SaveFormAsync(int eventId, List<EventRequirementAnswerDTO> answers, List<EventRequirementImageDTO> images) => await _repository.SaveFormAsync(eventId, answers, images);
    public async Task<ActionResponse<EventRequirementAnswer>> GetByEventAndRequirementAsync(int eventId, int requirementId) => await _repository.GetByEventAndRequirementAsync(eventId, requirementId);
    public async Task<ActionResponse<bool>> ClearFieldAsync(int eventId, int requirementId) => await _repository.ClearFieldAsync(eventId, requirementId);
}
