using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
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

        public Task<ActionResponse<bool>> ConsumeCreditAsync(string userId) => _repository.ConsumeCreditAsync(userId);

        public async Task<ActionResponse<UserCreditDTO>> GetByUserIdAsync(string userId) => await _repository.GetByUserIdAsync(userId);

        public async Task<ActionResponse<IEnumerable<UserCreditHistoryDTO>>> GetHistoryAsync(string userId) => await _repository.GetHistoryAsync(userId);

        public async Task<ActionResponse<IEnumerable<UserCreditDTO>>> GetPlannersWithCreditsAsync() => await _repository.GetPlannersWithCreditsAsync();
    }
}