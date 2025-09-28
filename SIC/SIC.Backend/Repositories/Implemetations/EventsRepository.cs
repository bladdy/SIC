using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class EventsRepository : GenericRepository<Event>, IEventsRepository
{
    private readonly DataContext _context;

    public EventsRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<ActionResponse<Event>> GetAsync(int id)
    {
        var events = await _context.Events.Include(et => et.EventType).FirstOrDefaultAsync(x => x.Id == id);
        if (events == null)
        {
            return new ActionResponse<Event>
            {
                Success = true,
                Message = "Evento no existe."
            };
        }
        return new ActionResponse<Event>
        {
            Success = true,
            Result = events
        };
    }

    public override async Task<ActionResponse<IEnumerable<Event>>> GetAsync()
    {
        var events = await _context.Events.Include(et => et.EventType).ToListAsync();
        return new ActionResponse<IEnumerable<Event>>
        {
            Success = true,
            Result = events
        };
    }
}