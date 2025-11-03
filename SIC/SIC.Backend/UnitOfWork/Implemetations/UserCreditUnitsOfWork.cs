using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class UserCreditUnitsOfWork : IUserCreditUnitsOfWork
    {
        private readonly IUserCreditRepository _repository;

        public UserCreditUnitsOfWork(IUserCreditRepository repository)
        {
            _repository = repository;
        }

        public async Task<ActionResponse<UserCreditDTO>> AddAsync(AddCreditsRequest entity) => await _repository.AddAsync(entity);

        public Task<ActionResponse<bool>> ConsumeCreditAsync(string userId, string EventName) => _repository.ConsumeCreditAsync(userId, EventName);

        public async Task<ActionResponse<UserCreditDTO>> GetByUserIdAsync(string userId) => await _repository.GetByUserIdAsync(userId);

        public async Task<ActionResponse<IEnumerable<UserCreditHistoryDTO>>> GetHistoryAsync(string userId) => await _repository.GetHistoryAsync(userId);

        public async Task<ActionResponse<IEnumerable<UserCreditDTO>>> GetPlannersWithCreditsAsync() => await _repository.GetPlannersWithCreditsAsync();

        public async Task<ActionResponse<IEnumerable<UserCreditHistory>>> GetAsync(PaginationDTO pagination) => await _repository.GetAsync(pagination);

        public async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination) => await _repository.GetTotalRecordAsync(pagination);
    }
}