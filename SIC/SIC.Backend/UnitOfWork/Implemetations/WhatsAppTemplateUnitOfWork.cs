using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class WhatsAppTemplateUnitOfWork : IWhatsAppTemplateUnitOfWork
    {
        private readonly IWhatsAppTemplateRepository _whatsAppTemplateRepository;

        public WhatsAppTemplateUnitOfWork(IWhatsAppTemplateRepository whatsAppTemplateRepository)
        {
            _whatsAppTemplateRepository = whatsAppTemplateRepository;
        }

        public Task<WhatsAppTemplate?> GetByNameAsync(string name, string? userId) => _whatsAppTemplateRepository.GetByNameAsync(name, userId);

        public Task<ActionResponse<bool>> DeleteByNameAsync(string name, string userId) => _whatsAppTemplateRepository.DeleteByNameAsync(name, userId);
    }
}