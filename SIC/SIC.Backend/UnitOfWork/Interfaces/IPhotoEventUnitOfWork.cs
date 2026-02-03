using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IPhotoEventUnitOfWork
    {
        Task<ActionResponse<bool>> AddFullPhotoEvenAsyn(PhotoEvent photoEvent);

        Task<ActionResponse<bool>> RemoveFullAsyn(int id);

        Task<ActionResponse<PhotoEvent>> GetByIdAsync(string code);

        Task<ActionResponse<bool>> AddFullImageAsyn(PhotoEventImage photoEventImage);
    }
}