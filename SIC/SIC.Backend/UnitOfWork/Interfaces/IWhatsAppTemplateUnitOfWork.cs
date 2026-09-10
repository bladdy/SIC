using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IWhatsAppTemplateUnitOfWork
    {
        Task<WhatsAppTemplate?> GetByNameAsync(string name, string? userId);

        Task<ActionResponse<bool>> DeleteByNameAsync(string name, string userId);
    }
}