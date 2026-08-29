using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
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

        var activity = await _context.MbMActivities
            .Include(a => a.Tasks)
            .FirstOrDefaultAsync(a => a.Id == task.MbMActivityId);

        if (activity != null && activity.Status != ActivityStatus.Cancelada)
        {
            var allDone = activity.Tasks.Any() && activity.Tasks.All(t => t.IsCompleted);
            if (allDone)
            {
                activity.Status = ActivityStatus.Completada;
            }
            else if (activity.Status == ActivityStatus.Completada)
            {
                activity.Status = ActivityStatus.Pendiente;
            }
            _context.Update(activity);
        }

        await _context.SaveChangesAsync();

        return new ActionResponse<MbMTask>
        {
            Success = true,
            Result = task
        };
    }
}
