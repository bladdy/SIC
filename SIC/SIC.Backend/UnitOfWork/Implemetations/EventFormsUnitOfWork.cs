using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class EventFormsUnitOfWork : GenericUnitOfWork<EventForm>, IEventFormsUnitOfWork
{
    private readonly IEventFormsRepository _repository;

    public EventFormsUnitOfWork(IGenericRepository<EventForm> genericRepository, IEventFormsRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<EventFormDTO>> GetByEventCodeAsync(string eventCode) => await _repository.GetByEventCodeAsync(eventCode);
    public async Task<ActionResponse<bool>> SaveAsync(int eventId, SaveEventFormDTO dto) => await _repository.SaveAsync(eventId, dto);
    public async Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> GetResponsesByEventAsync(string eventCode) => await _repository.GetResponsesByEventAsync(eventCode);
    public async Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> GetResponseByInvitationAsync(string invitationCode) => await _repository.GetResponseByInvitationAsync(invitationCode);
    public async Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> SaveResponseAsync(SubmitEventFormDTO dto) => await _repository.SaveResponseAsync(dto);
    public async Task<ActionResponse<bool>> ResetResponseAsync(int responseId) => await _repository.ResetResponseAsync(responseId);
    public async Task<ActionResponse<bool>> ExistsAsync(string eventCode) => await _repository.ExistsAsync(eventCode);
}