using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class MbMTaskUnitOfWork : GenericUnitOfWork<MbMTask>, IMbMTaskUnitOfWork
{
    private readonly IMbMTaskRepository _repository;

    public MbMTaskUnitOfWork(IGenericRepository<MbMTask> genericRepository, IMbMTaskRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<IEnumerable<MbMTask>>> GetByActivityIdAsync(int activityId)
        => await _repository.GetByActivityIdAsync(activityId);

    public async Task<ActionResponse<MbMTask>> ToggleCompleteAsync(int taskId)
        => await _repository.ToggleCompleteAsync(taskId);
}
