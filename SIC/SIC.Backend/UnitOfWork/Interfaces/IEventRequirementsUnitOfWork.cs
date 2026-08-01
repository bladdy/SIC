using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IEventRequirementsUnitOfWork : IGenericUnitOfWork<EventRequirement>
{
    Task<ActionResponse<IEnumerable<EventRequirement>>> GetBySectionAsync(string section);
}
