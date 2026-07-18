using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IMbMTaskRepository : IGenericRepository<MbMTask>
{
    Task<ActionResponse<IEnumerable<MbMTask>>> GetByActivityIdAsync(int activityId);

    Task<ActionResponse<MbMTask>> ToggleCompleteAsync(int taskId);
}
