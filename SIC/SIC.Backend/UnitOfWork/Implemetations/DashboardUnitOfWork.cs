using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class DashboardUnitOfWork : IDashboardUnitOfWork
    {
        private readonly IDashboardReporsitory _dashboardRepository;

        public DashboardUnitOfWork(IDashboardReporsitory dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<ActionResponse<AdminDashboardDto>> GetAdminDashboardAsync(string userId) => await _dashboardRepository.GetAdminDashboardAsync(userId);

        public async Task<ActionResponse<PlannerDashboardDto>> GetPlannerDashboardAsync(string userId) => await _dashboardRepository.GetPlannerDashboardAsync(userId);

        public async Task<ActionResponse<UserDashboardDto>> GetUserDashboardAsync(string userId) => await _dashboardRepository.GetUserDashboardAsync(userId);
    }
}