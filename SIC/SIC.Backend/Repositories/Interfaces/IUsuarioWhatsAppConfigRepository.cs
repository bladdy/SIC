using SIC.Shared.Entities;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IUsuarioWhatsAppConfigRepository
    {
        Task<UsuarioWhatsAppConfig?> GetByPhoneNumberIdAsync(string phoneNumberId);

        Task AddAsync(UsuarioWhatsAppConfig config);
    }
}