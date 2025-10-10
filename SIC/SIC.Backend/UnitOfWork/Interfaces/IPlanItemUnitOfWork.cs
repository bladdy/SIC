using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IPlanItemUnitOfWork
{
    Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);

    Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync(PaginationDTO paginations);

    Task<ActionResponse<IEnumerable<PlanItem>>> GetByIdAsync(int id);

    Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync();

    Task<ActionResponse<PlanItem>> AddOrUpdateFullAsync(List<int> items, int planId);
}