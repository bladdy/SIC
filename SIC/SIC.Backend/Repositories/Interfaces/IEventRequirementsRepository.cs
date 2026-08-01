using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IEventRequirementsRepository : IGenericRepository<EventRequirement>
{
    Task<ActionResponse<IEnumerable<EventRequirement>>> GetBySectionAsync(string section);
}
