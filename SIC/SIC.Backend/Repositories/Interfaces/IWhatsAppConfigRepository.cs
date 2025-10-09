using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IWhatsAppConfigRepository
    {
        Task<ActionResponse<UsuarioWhatsAppConfig>> GetByUserIdAsync(string userId);

        Task<ActionResponse<UsuarioWhatsAppConfig>> AddFullAsync(UsuarioWhatsAppConfig whatsAppConfig);

        Task<ActionResponse<UsuarioWhatsAppConfig>> UpdateFullAsync(UsuarioWhatsAppConfig whatsAppConfig);

        Task<ActionResponse<MassiveShippingProgress?>> GetByEventoUserAsync(int eventoId);
    }
}