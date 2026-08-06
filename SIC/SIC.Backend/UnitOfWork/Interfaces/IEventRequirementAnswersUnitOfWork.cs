using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IEventRequirementAnswersUnitOfWork : IGenericUnitOfWork<EventRequirementAnswer>
{
    Task<ActionResponse<IEnumerable<EventRequirementAnswer>>> GetByEventIdAsync(int eventId);
    Task<ActionResponse<bool>> SaveAllAsync(int eventId, List<EventRequirementAnswerDTO> answers);
    Task<ActionResponse<SaveFormResponseDTO>> SaveFormAsync(int eventId, List<EventRequirementAnswerDTO> answers, List<EventRequirementImageDTO> images);
    Task<ActionResponse<EventRequirementAnswer>> GetByEventAndRequirementAsync(int eventId, int requirementId);
    Task<ActionResponse<bool>> ClearFieldAsync(int eventId, int requirementId);
}
