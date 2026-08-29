using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class MinuteByMinuteRepository : GenericRepository<MinuteByMinute>, IMinuteByMinuteRepository
{
    private readonly DataContext _context;

    public MinuteByMinuteRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<MinuteByMinute>> GetByEventIdAsync(int eventId)
    {
        var minuteByMinute = await _context.MinuteByMinutes
            .Include(e => e.Event)
            .Include(e => e.Activities)
                .ThenInclude(a => a.Providers)
            .Include(e => e.Activities)
                .ThenInclude(a => a.Tasks)
            .FirstOrDefaultAsync(x => x.EventId == eventId);

        if (minuteByMinute == null)
        {
            return new ActionResponse<MinuteByMinute>
            {
                Success = true,
                Message = "Este evento no tiene Minute by Minute."
            };
        }

        return new ActionResponse<MinuteByMinute>
        {
            Success = true,
            Result = minuteByMinute
        };
    }

    public async Task<ActionResponse<MinuteByMinute>> GetByEventCodeAsync(string code)
    {
        var minuteByMinute = await _context.MinuteByMinutes
            .Include(e => e.Event)
                .ThenInclude(et => et!.EventType)
            .Include(e => e.Activities)
                .ThenInclude(a => a.Providers)
            .Include(e => e.Activities)
                .ThenInclude(a => a.Tasks)
            .FirstOrDefaultAsync(x => x.Event!.Code == code);

        if (minuteByMinute == null)
        {
            return new ActionResponse<MinuteByMinute>
            {
                Success = true,
                Message = "Este evento no tiene Minute by Minute."
            };
        }

        return new ActionResponse<MinuteByMinute>
        {
            Success = true,
            Result = minuteByMinute
        };
    }

    public async Task<ActionResponse<MinuteByMinute>> CreateForEventAsync(MinuteByMinute minuteByMinute, int eventId)
    {
        try
        {
            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null)
            {
                return new ActionResponse<MinuteByMinute>
                {
                    Success = false,
                    Message = "El evento no existe."
                };
            }

            var existing = await _context.MinuteByMinutes.FirstOrDefaultAsync(x => x.EventId == eventId);
            if (existing != null)
            {
                return new ActionResponse<MinuteByMinute>
                {
                    Success = false,
                    Message = "Este evento ya tiene un Minute by Minute."
                };
            }

            minuteByMinute.EventId = eventId;
            minuteByMinute.CreatedAt = DateTime.UtcNow;
            _context.Add(minuteByMinute);
            await _context.SaveChangesAsync();

            return new ActionResponse<MinuteByMinute>
            {
                Success = true,
                Result = minuteByMinute
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<MinuteByMinute>
            {
                Success = false,
                Message = exception.Message
            };
        }
    }
}
