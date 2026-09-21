using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IEventFormsRepository : IGenericRepository<EventForm>
{
    Task<ActionResponse<EventFormDTO>> GetByEventCodeAsync(string eventCode);
    Task<ActionResponse<bool>> SaveAsync(int eventId, SaveEventFormDTO dto);
    Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> GetResponsesByEventAsync(string eventCode);
    Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> GetResponseByInvitationAsync(string invitationCode);
    Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> SaveResponseAsync(SubmitEventFormDTO dto);
    Task<ActionResponse<bool>> ResetResponseAsync(int responseId);
    Task<ActionResponse<bool>> ExistsAsync(string eventCode);
}