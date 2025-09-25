using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class PlanUnitOfWork : GenericUnitOfWork<Plan>, IPlanUnitOfWork
{
    private readonly IPlanRepository _planRepository;

    public PlanUnitOfWork(IGenericRepository<Plan> repository, IPlanRepository planRepository) : base(repository)
    {
        _planRepository = planRepository;
    }
    public override async Task<ActionResponse<IEnumerable<Plan>>> GetAsync() => await _planRepository.GetAsync();    
    public override async Task<ActionResponse<Plan>> GetAsync(int id)=> await _planRepository.GetAsync(id);
}
