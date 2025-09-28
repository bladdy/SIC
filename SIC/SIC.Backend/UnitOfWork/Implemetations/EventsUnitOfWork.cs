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

    public async Task<ActionResponse<Event>> GetByCodeAsync(string code) => await _eventstRepository.GetByCodeAsync(code);

    public override async Task<ActionResponse<IEnumerable<Event>>> GetAsync() => await _eventstRepository.GetAsync();

    public async Task<ActionResponse<Event>> AddFullAsync(Event events) => await _eventstRepository.AddFullAsync(events);

    public async Task<ActionResponse<Event>> UpdateFullAsync(Event events) => await _eventstRepository.UpdateFullAsync(events);
}