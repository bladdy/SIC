using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IWhatsAppTemplateRepository
    {
        Task<ActionResponse<IEnumerable<WhatsAppTemplate?>>> GetAllAsync();

        Task<WhatsAppTemplate?> GetByNameAsync(string name);
    }
}