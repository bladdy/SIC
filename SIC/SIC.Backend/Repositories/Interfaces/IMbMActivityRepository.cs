using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IMbMActivityRepository : IGenericRepository<MbMActivity>
{
    Task<ActionResponse<MbMActivity>> GetWithDetailsAsync(int id);

    Task<ActionResponse<IEnumerable<MbMActivity>>> GetByMinuteByMinuteIdAsync(int minuteByMinuteId);
}
