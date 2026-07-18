using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class MinuteByMinuteUnitOfWork : GenericUnitOfWork<MinuteByMinute>, IMinuteByMinuteUnitOfWork
{
    private readonly IMinuteByMinuteRepository _minuteByMinuteRepository;

    public MinuteByMinuteUnitOfWork(IGenericRepository<MinuteByMinute> repository, IMinuteByMinuteRepository minuteByMinuteRepository)
        : base(repository)
    {
        _minuteByMinuteRepository = minuteByMinuteRepository;
    }

    public async Task<ActionResponse<MinuteByMinute>> GetByEventIdAsync(int eventId)
        => await _minuteByMinuteRepository.GetByEventIdAsync(eventId);

    public async Task<ActionResponse<MinuteByMinute>> GetByEventCodeAsync(string code)
        => await _minuteByMinuteRepository.GetByEventCodeAsync(code);

    public async Task<ActionResponse<MinuteByMinute>> CreateForEventAsync(MinuteByMinute minuteByMinute, int eventId)
        => await _minuteByMinuteRepository.CreateForEventAsync(minuteByMinute, eventId);
}
