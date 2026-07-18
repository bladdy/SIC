using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IMinuteByMinuteRepository : IGenericRepository<MinuteByMinute>
{
    Task<ActionResponse<MinuteByMinute>> GetByEventIdAsync(int eventId);

    Task<ActionResponse<MinuteByMinute>> GetByEventCodeAsync(string code);

    Task<ActionResponse<MinuteByMinute>> CreateForEventAsync(MinuteByMinute minuteByMinute, int eventId);
}
