using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IEventRequirementAnswersRepository : IGenericRepository<EventRequirementAnswer>
{
    Task<ActionResponse<IEnumerable<EventRequirementAnswer>>> GetByEventIdAsync(int eventId);
    Task<ActionResponse<bool>> SaveAllAsync(int eventId, List<EventRequirementAnswerDTO> answers);
    Task<ActionResponse<SaveFormResponseDTO>> SaveFormAsync(int eventId, List<EventRequirementAnswerDTO> answers, List<EventRequirementImageDTO> images);
}
