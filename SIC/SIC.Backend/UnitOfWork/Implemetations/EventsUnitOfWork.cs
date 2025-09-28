using SIC.Backend.Data;
using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class EventsUnitOfWork : GenericUnitOfWork<Event>, IEventsUnitOfWork
{
    private readonly IEventsRepository _eventstRepository;

    public EventsUnitOfWork(IGenericRepository<Event> repository, IEventsRepository eventstRepository) : base(repository)
    {
        _eventstRepository = eventstRepository;
    }

    public override async Task<ActionResponse<Event>> GetAsync(int id) => await _eventstRepository.GetAsync(id);

    public override async Task<ActionResponse<IEnumerable<Event>>> GetAsync() => await _eventstRepository.GetAsync();
}