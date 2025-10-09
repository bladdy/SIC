using DocumentFormat.OpenXml.Spreadsheet;
using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class WhatsAppConfigUnitOfWork : GenericUnitOfWork<UsuarioWhatsAppConfig>, IWhatsAppConfigUnitOfWork
    {
        private readonly IWhatsAppConfigRepository _WhatsAppConfigUnitOfWork;

        public WhatsAppConfigUnitOfWork(IGenericRepository<UsuarioWhatsAppConfig> repository, IWhatsAppConfigRepository WhatsAppConfigUnitOfWork) : base(repository)
        {
            _WhatsAppConfigUnitOfWork = WhatsAppConfigUnitOfWork;
        }

        public async Task<ActionResponse<UsuarioWhatsAppConfig>> GetByUserIdAsync(string userId) => await _WhatsAppConfigUnitOfWork.GetByUserIdAsync(userId);

        public async Task<ActionResponse<UsuarioWhatsAppConfig>> AddFullAsync(UsuarioWhatsAppConfig whatsAppConfig) => await _WhatsAppConfigUnitOfWork.AddFullAsync(whatsAppConfig);

        public async Task<ActionResponse<UsuarioWhatsAppConfig>> UpdateFullAsync(UsuarioWhatsAppConfig whatsAppConfig) => await _WhatsAppConfigUnitOfWork.UpdateFullAsync(whatsAppConfig);

        public async Task<ActionResponse<MassiveShippingProgress?>> GetByEventoUserAsync(int eventoId) => await _WhatsAppConfigUnitOfWork.GetByEventoUserAsync(eventoId);
    }
}