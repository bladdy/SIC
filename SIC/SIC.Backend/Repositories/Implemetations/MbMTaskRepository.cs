using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class MbMTaskRepository : GenericRepository<MbMTask>, IMbMTaskRepository
{
    private readonly DataContext _context;

    public MbMTaskRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<IEnumerable<MbMTask>>> GetByActivityIdAsync(int activityId)
    {
        var tasks = await _context.MbMTasks
            .Where(t => t.MbMActivityId == activityId)
            .ToListAsync();

        return new ActionResponse<IEnumerable<MbMTask>>
        {
            Success = true,
            Result = tasks
        };
    }

    public async Task<ActionResponse<MbMTask>> ToggleCompleteAsync(int taskId)
    {
        var task = await _context.MbMTasks.FindAsync(taskId);
        if (task == null)
        {
            return new ActionResponse<MbMTask>
            {
                Success = false,
                Message = "La tarea no existe."
            };
        }

        task.IsCompleted = !task.IsCompleted;
        _context.Update(task);
        await _context.SaveChangesAsync();

        return new ActionResponse<MbMTask>
        {
            Success = true,
            Result = task
        };
    }
}
