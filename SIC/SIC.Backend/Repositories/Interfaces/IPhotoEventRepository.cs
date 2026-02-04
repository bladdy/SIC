using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IPhotoEventRepository
    {
        Task<ActionResponse<bool>> AddFullPhotoEvenAsyn(PhotoEvent photoEvent);

        Task<ActionResponse<bool>> RemoveFullAsyn(int id);

        Task<ActionResponse<PhotoEvent>> GetByIdAsync(string code);

        Task<ActionResponse<bool>> AddFullImageAsyn(PhotoEventImage photoEventImage);

        Task<ActionResponse<IEnumerable<PhotoEventImage>>> GetByImagenCodeAsync(string code);
    }
}