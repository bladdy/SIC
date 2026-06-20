using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIC.Backend.Repositories.Interfaces
{
    //ToDo: Implement Pending Pagination and Filtering
    public interface IUserCreditRepository
    {
        Task<ActionResponse<UserCreditDTO>> GetByUserIdAsync(string userId);

        Task<ActionResponse<UserCreditDTO>> AddAsync(AddCreditsRequest entity);

        Task<ActionResponse<bool>> ConsumeCreditAsync(string userId, string EventName);

        Task<ActionResponse<IEnumerable<UserCreditHistory>>> GetAsync(PaginationDTO pagination);

        Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);

        Task<ActionResponse<IEnumerable<UserCreditDTO>>> GetPlannersWithCreditsAsync();

        Task<ActionResponse<IEnumerable<UserCreditHistoryDTO>>> GetHistoryAsync(string userId);

        Task<ActionResponse<StripeEventLog>> AddStripeEventLogAsync(StripeEventLog entity);
        Task<ActionResponse<bool>> ExistStripeEventLogAsync(string id);
    }
}