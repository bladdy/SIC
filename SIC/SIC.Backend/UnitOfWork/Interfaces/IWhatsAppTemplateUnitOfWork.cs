using SIC.Shared.Entities;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IWhatsAppTemplateUnitOfWork
    {
        Task<WhatsAppTemplate?> GetByNameAsync(string name);
    }
}