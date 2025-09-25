using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IPlanRepository
    {
        Task<ActionResponse<Plan>> GetAsync(int id);
        Task<ActionResponse<IEnumerable<Plan>>> GetAsync();
    }
}
