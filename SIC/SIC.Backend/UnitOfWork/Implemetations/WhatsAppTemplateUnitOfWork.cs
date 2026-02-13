using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class WhatsAppTemplateUnitOfWork : IWhatsAppTemplateUnitOfWork
    {
        private readonly IWhatsAppTemplateRepository _whatsAppTemplateRepository;

        public WhatsAppTemplateUnitOfWork(IWhatsAppTemplateRepository whatsAppTemplateRepository)
        {
            _whatsAppTemplateRepository = whatsAppTemplateRepository;
        }

        public Task<WhatsAppTemplate?> GetByNameAsync(string name) => _whatsAppTemplateRepository.GetByNameAsync(name);
    }
}