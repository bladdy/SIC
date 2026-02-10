using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class UsuarioWhatsAppConfigUnitOfWork : IUsuarioWhatsAppConfigUnitOfWork
    {
        public readonly IUsuarioWhatsAppConfigRepository _repository;

        public UsuarioWhatsAppConfigUnitOfWork(IUsuarioWhatsAppConfigRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(UsuarioWhatsAppConfig config) => await _repository.AddAsync(config);

        public Task<UsuarioWhatsAppConfig?> GetByPhoneNumberIdAsync(string phoneNumberId) => _repository.GetByPhoneNumberIdAsync(phoneNumberId);
    }
}