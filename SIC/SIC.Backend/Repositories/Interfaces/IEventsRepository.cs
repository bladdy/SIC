using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IEventsRepository
{
    Task<ActionResponse<Event>> GetAsync(int id);

    Task<ActionResponse<IEnumerable<Event>>> GetAsync();
}