using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IPlanItemRepository
    {
        Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync(PaginationDTO paginations);
        Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);
        Task<ActionResponse<PlanItem>> GetAsync(int id);
        Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync();
    }
}
