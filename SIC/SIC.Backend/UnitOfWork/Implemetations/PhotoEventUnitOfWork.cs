using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class PhotoEventUnitOfWork : IPhotoEventUnitOfWork
    {
        public readonly IPhotoEventRepository _photoEventRepository;

        public PhotoEventUnitOfWork(IPhotoEventRepository photoEventRepository)
        {
            _photoEventRepository = photoEventRepository;
        }

        public async Task<ActionResponse<bool>> AddFullImageAsyn(PhotoEventImage photoEventImage) => await _photoEventRepository.AddFullImageAsyn(photoEventImage);

        public async Task<ActionResponse<bool>> AddFullPhotoEvenAsyn(PhotoEvent photoEvent) => await _photoEventRepository.AddFullPhotoEvenAsyn(photoEvent);

        public async Task<ActionResponse<PhotoEvent>> GetByIdAsync(string code) => await _photoEventRepository.GetByIdAsync(code);

        public async Task<ActionResponse<bool>> RemoveFullAsyn(int id) => await _photoEventRepository.RemoveFullAsyn(id);
    }
}