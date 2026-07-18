using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IMbMTaskUnitOfWork : IGenericUnitOfWork<MbMTask>
{
    Task<ActionResponse<IEnumerable<MbMTask>>> GetByActivityIdAsync(int activityId);

    Task<ActionResponse<MbMTask>> ToggleCompleteAsync(int taskId);
}
