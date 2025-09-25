using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IPlanItemUnitOfWork
{
    Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);
    Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync(PaginationDTO paginations);
    Task<ActionResponse<PlanItem>> GetAsync(int id);
    Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync();
}
