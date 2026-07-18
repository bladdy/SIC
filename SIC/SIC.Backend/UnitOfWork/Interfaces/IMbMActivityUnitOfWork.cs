using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IMbMActivityUnitOfWork : IGenericUnitOfWork<MbMActivity>
{
    Task<ActionResponse<MbMActivity>> GetWithDetailsAsync(int id);

    Task<ActionResponse<IEnumerable<MbMActivity>>> GetByMinuteByMinuteIdAsync(int minuteByMinuteId);
}
