using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IMbMProviderUnitOfWork : IGenericUnitOfWork<MbMProvider>
{
    Task<ActionResponse<IEnumerable<MbMProvider>>> GetByActivityIdAsync(int activityId);
}
