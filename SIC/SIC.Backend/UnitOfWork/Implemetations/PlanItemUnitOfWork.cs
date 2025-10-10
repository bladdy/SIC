using DocumentFormat.OpenXml.Office2010.Excel;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class PlanItemUnitOfWork : GenericUnitOfWork<PlanItem>, IPlanItemUnitOfWork
{
    private readonly IPlanItemRepository _planRepository;

    public PlanItemUnitOfWork(IGenericRepository<PlanItem> repository, IPlanItemRepository planRepository) : base(repository)
    {
        _planRepository = planRepository;
    }

    public override async Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync(PaginationDTO pagination) => await _planRepository.GetAsync(pagination);

    public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination) => await _planRepository.GetTotalRecordAsync(pagination);

    public override async Task<ActionResponse<IEnumerable<PlanItem>>> GetAsync() => await _planRepository.GetAsync();

    public async Task<ActionResponse<IEnumerable<PlanItem>>> GetByIdAsync(int id) => await _planRepository.GetByIdAsync(id);

    public async Task<ActionResponse<PlanItem>> AddOrUpdateFullAsync(List<int> items, int planId) => await _planRepository.AddOrUpdateFullAsync(items, planId);
}