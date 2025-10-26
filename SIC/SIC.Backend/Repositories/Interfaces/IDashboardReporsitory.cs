using DocumentFormat.OpenXml.Spreadsheet;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IDashboardReporsitory
{
    Task<ActionResponse<AdminDashboardDto>> GetAdminDashboardAsync(string userId);

    Task<ActionResponse<PlannerDashboardDto>> GetPlannerDashboardAsync(string userId);

    Task<ActionResponse<UserDashboardDto>> GetUserDashboardAsync(string userId);
}