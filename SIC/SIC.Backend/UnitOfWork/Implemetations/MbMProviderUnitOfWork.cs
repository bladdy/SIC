using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class MbMProviderUnitOfWork : GenericUnitOfWork<MbMProvider>, IMbMProviderUnitOfWork
{
    private readonly IMbMProviderRepository _repository;

    public MbMProviderUnitOfWork(IGenericRepository<MbMProvider> genericRepository, IMbMProviderRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<IEnumerable<MbMProvider>>> GetByActivityIdAsync(int activityId)
        => await _repository.GetByActivityIdAsync(activityId);
}
