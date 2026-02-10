using SIC.Shared.Entities;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IUsuarioWhatsAppConfigUnitOfWork
    {
        Task<UsuarioWhatsAppConfig?> GetByPhoneNumberIdAsync(string phoneNumberId);

        Task AddAsync(UsuarioWhatsAppConfig config);
    }
}