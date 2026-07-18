using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IMinuteByMinuteUnitOfWork : IGenericUnitOfWork<MinuteByMinute>
{
    Task<ActionResponse<MinuteByMinute>> GetByEventIdAsync(int eventId);

    Task<ActionResponse<MinuteByMinute>> GetByEventCodeAsync(string code);

    Task<ActionResponse<MinuteByMinute>> CreateForEventAsync(MinuteByMinute minuteByMinute, int eventId);
}
