using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations
{
    public class ImagesRepository : IImagesRepository
    {
        private readonly DataContext _context;

        public ImagesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ActionResponse<EventImage>> AddFullAsyn(EventImageDTO eventImage)
        {
            try
            {
                if (eventImage == null)
                {
                    return new ActionResponse<EventImage>
                    {
                        Success = false,
                        Message = "El objeto de imagen del evento es nulo."
                    };
                }
                var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Code == eventImage.CodeEvent);
                if (eventEntity == null)
                {
                    return new ActionResponse<EventImage>
                    {
                        Success = false,
                        Message = "El evento asociado no existe."
                    };
                }
                var newEventImage = new EventImage
                {
                    EventId = eventEntity.Id,
                    Url = eventImage.ImageUrl,
                    FileName = eventImage.FileName,
                    ImageType = eventImage.ImageType
                };
                _context.EventImages.Add(newEventImage);
                await _context.SaveChangesAsync();
                return new ActionResponse<EventImage>
                {
                    Success = true,
                    Result = newEventImage
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<EventImage>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<EventImage>> DeleteAsync(int id)
        {
            var imagen = await _context.EventImages.FindAsync(id);
            if (imagen == null)
            {
                return new ActionResponse<EventImage>
                {
                    Success = true,
                    Message = "No existen imágenes para este evento."
                };
            }
            try
            {
                _context.Remove(imagen);
                await _context.SaveChangesAsync();
                return new ActionResponse<EventImage>
                {
                    Success = true,
                };
            }
            catch (Exception)
            {
                return new ActionResponse<EventImage>
                {
                    Success = true,
                    Message = "Algo salio mal, intentalo más tarde."
                };
            }
        }

        public async Task<ActionResponse<IEnumerable<EventImage>>> GetAsync(string code)
        {
            var eventImages = await _context.EventImages.Include(I => I.Event)
                .Where(ei => ei.Event.Code == code).OrderByDescending(ei => ei.PostingDate)
                .ToListAsync();

            if (eventImages == null || !eventImages.Any())
            {
                return new ActionResponse<IEnumerable<EventImage>>
                {
                    Success = true,
                    Message = "No existen imágenes para este evento."
                };
            }

            return new ActionResponse<IEnumerable<EventImage>>
            {
                Success = true,
                Result = eventImages
            };
        }
    }
}