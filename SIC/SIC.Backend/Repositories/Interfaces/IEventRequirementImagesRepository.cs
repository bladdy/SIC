using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IEventRequirementImagesRepository : IGenericRepository<EventRequirementImage>
{
    Task<ActionResponse<IEnumerable<EventRequirementImage>>> GetByAnswerIdAsync(int answerId);
    Task<ActionResponse<IEnumerable<EventRequirementImage>>> GetByEventIdAsync(int eventId);
    Task<ActionResponse<EventRequirementImage>> GetByIdWithAnswerAsync(int id);
}
