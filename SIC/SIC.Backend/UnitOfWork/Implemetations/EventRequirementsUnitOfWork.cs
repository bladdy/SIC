using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class EventRequirementsUnitOfWork : GenericUnitOfWork<EventRequirement>, IEventRequirementsUnitOfWork
{
    private readonly IEventRequirementsRepository _repository;

    public EventRequirementsUnitOfWork(IGenericRepository<EventRequirement> genericRepository, IEventRequirementsRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<IEnumerable<EventRequirement>>> GetBySectionAsync(string section) => await _repository.GetBySectionAsync(section);
}
