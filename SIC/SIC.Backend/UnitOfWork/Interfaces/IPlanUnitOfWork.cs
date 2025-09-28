using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IPlanUnitOfWork
{
    Task<ActionResponse<Plan>> GetAsync(int id);

    Task<ActionResponse<IEnumerable<Plan>>> GetAsync();
}