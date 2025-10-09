using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IWhatsAppConfigUnitOfWork
    {
        Task<ActionResponse<UsuarioWhatsAppConfig>> GetByUserIdAsync(string userId);

        Task<ActionResponse<MassiveShippingProgress?>> GetByEventoUserAsync(int eventoId);

        Task<ActionResponse<UsuarioWhatsAppConfig>> AddFullAsync(UsuarioWhatsAppConfig whatsAppConfig);

        Task<ActionResponse<UsuarioWhatsAppConfig>> UpdateFullAsync(UsuarioWhatsAppConfig whatsAppConfig);
    }
}