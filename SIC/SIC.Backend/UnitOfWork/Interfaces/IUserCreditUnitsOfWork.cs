using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IUserCreditUnitsOfWork
    {
        Task<ActionResponse<UserCreditDTO>> GetByUserIdAsync(string userId);

        Task<ActionResponse<UserCreditDTO>> AddAsync(AddCreditsRequest entity);

        Task<ActionResponse<bool>> ConsumeCreditAsync(string userId);

        Task<ActionResponse<IEnumerable<UserCreditDTO>>> GetPlannersWithCreditsAsync();

        Task<ActionResponse<IEnumerable<UserCreditHistoryDTO>>> GetHistoryAsync(string userId);
    }
}