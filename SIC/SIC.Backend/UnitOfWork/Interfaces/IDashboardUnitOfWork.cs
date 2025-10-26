using SIC.Shared.DTOs;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IDashboardUnitOfWork
    {
        Task<ActionResponse<AdminDashboardDto>> GetAdminDashboardAsync(string userId);

        Task<ActionResponse<PlannerDashboardDto>> GetPlannerDashboardAsync(string userId);

        Task<ActionResponse<UserDashboardDto>> GetUserDashboardAsync(string userId);
    }
}