using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class ImageUnitOfWork : IImageUnitOfWork
    {
        public readonly IImagesRepository _imagesRepository;

        public ImageUnitOfWork(IImagesRepository imagesRepository)
        {
            _imagesRepository = imagesRepository;
        }

        public Task<ActionResponse<EventImage>> AddFullAsyn(EventImageDTO eventImage) => _imagesRepository.AddFullAsyn(eventImage);

        public Task<ActionResponse<EventImage>> DeleteAsync(int id) => _imagesRepository.DeleteAsync(id);

        public Task<ActionResponse<IEnumerable<EventImage>>> GetAsync(string code) => _imagesRepository.GetAsync(code);
    }
}