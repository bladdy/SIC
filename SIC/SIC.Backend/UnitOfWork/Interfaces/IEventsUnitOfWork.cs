using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IEventsUnitOfWork
{
    Task<ActionResponse<Event>> GetAsync(int id);

    Task<ActionResponse<IEnumerable<Event>>> GetAsync();
}