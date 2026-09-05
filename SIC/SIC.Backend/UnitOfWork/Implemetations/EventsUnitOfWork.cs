using SIC.Backend.Data;
using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
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

    public async Task<ActionResponse<EventInfoDto>> GetInfoByCodeAsync(string code) => await _eventstRepository.GetInfoByCodeAsync(code);

    public override async Task<ActionResponse<IEnumerable<Event>>> GetAsync() => await _eventstRepository.GetAsync();

    public async Task<ActionResponse<Event>> AddFullAsync(Event events) => await _eventstRepository.AddFullAsync(events);

    public async Task<ActionResponse<Event>> UpdateFullAsync(Event events) => await _eventstRepository.UpdateFullAsync(events);

    public async Task<ActionResponse<IEnumerable<Event>>> GetByUserIdAsync(string userId) => await _eventstRepository.GetByUserIdAsync(userId);

    public override async Task<ActionResponse<IEnumerable<Event>>> GetAsync(PaginationDTO pagination) => await _eventstRepository.GetAsync(pagination);

    public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination) => await _eventstRepository.GetTotalRecordAsync(pagination);

    public async Task<ActionResponse<IEnumerable<Event>>> GetActiveWithRequirementStatusAsync(PaginationDTO pagination) => await _eventstRepository.GetActiveWithRequirementStatusAsync(pagination);

    public async Task<ActionResponse<int>> GetActiveWithRequirementStatusTotalAsync(PaginationDTO pagination) => await _eventstRepository.GetActiveWithRequirementStatusTotalAsync(pagination);
}