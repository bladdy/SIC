using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
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

    public async Task<ActionResponse<Event>> GetByCodeAsync(string code)
    {
        var events = await _context.Events.Include(et => et.EventType).FirstOrDefaultAsync(x => x.Code.Contains(code));
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

    public async Task<ActionResponse<Event>> AddFullAsync(Event events)
    {
        try
        {
            var eventType = await _context.EventTypes.FirstOrDefaultAsync(x => x.Id == events.EventTypeId);
            if (eventType == null)
            {
                return new ActionResponse<Event>
                {
                    Success = false,
                    Message = "El Tipo de Evento no es valido."
                };
            }

            events.Code = CodeGenerator.GenerateCode();
            events.EventType = eventType;
            _context.Add(events);
            await _context.SaveChangesAsync();
            return new ActionResponse<Event>
            {
                Success = true,
                Result = events
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Event>
            {
                Success = false,
                Message = "Ya existe un Evento con el mismo Titulo, Sub-Titulo y Fecha."
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Event>
            {
                Success = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<Event>> UpdateFullAsync(Event events)
    {
        try
        {
            var eventType = await _context.EventTypes.FirstOrDefaultAsync(x => x.Id == events.EventTypeId);
            if (eventType == null)
            {
                return new ActionResponse<Event>
                {
                    Success = false,
                    Message = "El Tipo de Evento no es valido."
                };
            }

            events.EventType = eventType;
            _context.Update(events);
            await _context.SaveChangesAsync();
            return new ActionResponse<Event>
            {
                Success = true,
                Result = events
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Event>
            {
                Success = false,
                Message = "Ya existe un Evento con el mismo Titulo, Sub-Titulo y Fecha."
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Event>
            {
                Success = false,
                Message = exception.Message
            };
        }
    }
}