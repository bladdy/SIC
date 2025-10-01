using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IEventsRepository
{
    Task<ActionResponse<Event>> GetByCodeAsync(string code);

    Task<ActionResponse<IEnumerable<Event>>> GetByUserIdAsync(string userId);

    Task<ActionResponse<IEnumerable<Event>>> GetAsync();

    Task<ActionResponse<Event>> AddFullAsync(Event events);

    Task<ActionResponse<Event>> UpdateFullAsync(Event events);
}