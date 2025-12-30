using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IImagesRepository
    {
        Task<ActionResponse<IEnumerable<EventImage>>> GetAsync(string code);

        Task<ActionResponse<EventImage>> AddFullAsyn(EventImageDTO eventImage);

        Task<ActionResponse<EventImage>> DeleteAsync(int id);
    }
}