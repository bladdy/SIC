using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IMbMProviderRepository : IGenericRepository<MbMProvider>
{
    Task<ActionResponse<IEnumerable<MbMProvider>>> GetByActivityIdAsync(int activityId);
}
