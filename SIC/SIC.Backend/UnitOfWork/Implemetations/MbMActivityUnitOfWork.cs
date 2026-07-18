using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class MbMActivityUnitOfWork : GenericUnitOfWork<MbMActivity>, IMbMActivityUnitOfWork
{
    private readonly IMbMActivityRepository _repository;

    public MbMActivityUnitOfWork(IGenericRepository<MbMActivity> genericRepository, IMbMActivityRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<MbMActivity>> GetWithDetailsAsync(int id)
        => await _repository.GetWithDetailsAsync(id);

    public async Task<ActionResponse<IEnumerable<MbMActivity>>> GetByMinuteByMinuteIdAsync(int minuteByMinuteId)
        => await _repository.GetByMinuteByMinuteIdAsync(minuteByMinuteId);
}
