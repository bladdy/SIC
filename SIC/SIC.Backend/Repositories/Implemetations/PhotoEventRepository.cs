using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations
{
    public class PhotoEventRepository : IPhotoEventRepository
    {
        private readonly DataContext _context;

        public PhotoEventRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ActionResponse<bool>> AddFullImageAsyn(PhotoEventImage photoEventImage)
        {
            try
            {
                if (photoEventImage == null)
                {
                    return new ActionResponse<bool>
                    {
                        Message = "El evento de fotos no puede ser nulo",
                        Success = false
                    };
                }
                await _context.PhotoEventImages.AddAsync(photoEventImage);
                await _context.SaveChangesAsync();

                return new ActionResponse<bool>
                {
                    Message = "El evento de fotos fue creado exitosamente.",
                    Success = true
                };
            }
            catch (Exception E)
            {
                return new ActionResponse<bool>
                {
                    Message = "Algo salio mal, intentalo más tarde.",
                    Success = false
                };
            }
        }

        public async Task<ActionResponse<bool>> AddFullPhotoEvenAsyn(PhotoEvent photoEvent)
        {
            try
            {
                if (photoEvent == null)
                {
                    return new ActionResponse<bool>
                    {
                        Message = "El evento de fotos no puede ser nulo",
                        Success = false
                    };
                }
                photoEvent.Code = CodeGenerator.GenerateCode();
                await _context.PhotoEvents.AddAsync(photoEvent);
                await _context.SaveChangesAsync();

                return new ActionResponse<bool>
                {
                    Message = "El evento de fotos fue creado exitosamente.",
                    Success = true
                };
            }
            catch (Exception)
            {
                return new ActionResponse<bool>
                {
                    Message = "Algo salio mal, intentalo más tarde.",
                    Success = false
                };
            }
        }

        public async Task<ActionResponse<PhotoEvent>> GetByIdAsync(string code)
        {
            var photoEvent = await _context.PhotoEvents.Include(pe => pe.Images)
                .FirstOrDefaultAsync(pe => pe.Code == code);
            if (photoEvent == null)
            {
                return new ActionResponse<PhotoEvent>
                {
                    Success = false,
                    Message = "El evento de fotos no existe."
                };
            }
            return new ActionResponse<PhotoEvent>
            {
                Success = true,
                Result = photoEvent
            };
        }

        public async Task<ActionResponse<bool>> RemoveFullAsyn(int id)
        {
            var photoEvent = await _context.PhotoEvents
                .Include(pe => pe.Images)
                .FirstOrDefaultAsync(pe => pe.Id == id);

            if (photoEvent == null)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "El evento de fotos no existe."
                };
            }

            try
            {
                // 1️⃣ Eliminar imágenes relacionadas
                if (photoEvent.Images.Any())
                {
                    _context.PhotoEventImages.RemoveRange(photoEvent.Images);
                }

                // 2️⃣ Eliminar evento principal
                _context.PhotoEvents.Remove(photoEvent);

                await _context.SaveChangesAsync();

                return new ActionResponse<bool>
                {
                    Success = true,
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "Algo salió mal, inténtalo más tarde."
                };
            }
        }
    }
}