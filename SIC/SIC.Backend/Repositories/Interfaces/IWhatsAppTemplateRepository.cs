using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IWhatsAppTemplateRepository
    {
        Task<ActionResponse<bool>> AddSentTemplateAsync(int templateNumber, int id);

        Task<ActionResponse<bool>> CreateTemplates(WhatsAppTemplate entity);

        Task<ActionResponse<IEnumerable<WhatsAppTemplate?>>> GetAllAsync();

        Task<ActionResponse<IEnumerable<WhatsAppTemplate?>>> GetForUserAsync(string? userId);

        Task<WhatsAppTemplate?> GetByNameAsync(string name, string? userId);

        Task<ActionResponse<bool>> DeleteByNameAsync(string name, string userId);

        Task<int> GetNextTemplateNumberAsync(string userId);
    }
}