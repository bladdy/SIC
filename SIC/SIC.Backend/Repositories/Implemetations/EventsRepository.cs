using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
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
        var events = await _context.Events
            .Include(i => i.Invitations)
            .Include(et => et.EventType).FirstOrDefaultAsync(x => x.Code!.Contains(code));
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

    public async Task<ActionResponse<IEnumerable<Event>>> GetByUserIdAsync(string userId)
    {
        var events = await _context.Events.Include(i => i.Invitations)
            .Include(et => et.EventType)
            .OrderBy(e => e.Name)
            .Where(e => e.UserId!.ToString() == userId)
            .ToListAsync();
        return new ActionResponse<IEnumerable<Event>>
        {
            Success = true,
            Result = events
        };
    }

    public override async Task<ActionResponse<IEnumerable<Event>>> GetAsync()
    {
        var events = await _context.Events.Include(i => i.Invitations).Include(et => et.EventType).OrderBy(e => e.Name).ToListAsync();
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
            var confirmation = await _context.Templates.FirstOrDefaultAsync(e => e.Name.ToLower() == "Confirmacion");
            var aviso = await _context.Templates.FirstOrDefaultAsync(e => e.Name.ToLower() == "Aviso");
            events.Code = CodeGenerator.GenerateCode();
            events.EventType = eventType;
            events.Message = new Message
            {
                Title = events.Name,
                SubTitle = events.SubTitle,
                MessageInvitation = confirmation!.Content,
                MessageConfirmation = aviso!.Content,
            };

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

    public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
    {
        var queryable = _context.Events.Include(e => e.EventType).AsQueryable();
        if (!string.IsNullOrWhiteSpace(pagination.UserId))
            queryable = queryable.Where(x => x.UserId == pagination.UserId);
        if (!string.IsNullOrWhiteSpace(pagination.Filter))
        {
            var filter = pagination.Filter.ToLower();

            queryable = queryable.Where(x =>
                x.Name.ToLower().Contains(filter) ||
                x.SubTitle.ToLower().Contains(filter) ||
                x.Host.ToLower().Contains(filter) ||
                x.Planner!.ToLower().Contains(filter)
            );
        }

        if (pagination.Date.HasValue)
        {
            queryable = queryable.Where(x => x.Date.Date == pagination.Date.Value.Date);
        }
        if (pagination.EventTypeId > 0)
        {
            queryable = queryable.Where(x => x.EventTypeId == pagination.EventTypeId);
        }
        if (!string.IsNullOrWhiteSpace(pagination.OrderBy))
        {
            queryable = queryable.OrderByDescending(x => x.Date);
        }
        else
        {
            queryable = queryable.OrderBy(x => x.Date);
        }
        double count = await queryable.CountAsync();
        int totalPages = (int)Math.Ceiling(count / pagination.PageSize);
        return new ActionResponse<int>
        {
            Success = true,
            Result = totalPages
        };
    }

    public override async Task<ActionResponse<IEnumerable<Event>>> GetAsync(PaginationDTO pagination)
    {
        var queryable = _context.Events.Include(e => e.EventType).AsQueryable();
        if (!string.IsNullOrWhiteSpace(pagination.UserId))
            queryable = queryable.Where(x => x.UserId == pagination.UserId);
        if (!string.IsNullOrWhiteSpace(pagination.Filter))
        {
            var filter = pagination.Filter.ToLower();

            queryable = queryable.Where(x =>
                x.Name.ToLower().Contains(filter) ||
                x.SubTitle.ToLower().Contains(filter) ||
                x.Host.ToLower().Contains(filter) ||
                x.Planner!.ToLower().Contains(filter)
            );
        }
        if (!string.IsNullOrEmpty(pagination.OrderBy))
        {
            switch (pagination.OrderBy)
            {
                case "FechaAsc":
                    queryable = queryable.OrderBy(x => x.Date);
                    break;

                case "FechaDesc":
                    queryable = queryable.OrderByDescending(x => x.Date);
                    break;

                case "Nombre":
                    queryable = queryable.OrderBy(x => x.Name);
                    break;

                case "Evento":
                    queryable = queryable.OrderBy(x => x.EventType!.Name);
                    break;

                case "Estado":
                    queryable = queryable.OrderBy(x => x.Status);
                    queryable = queryable.OrderBy(x => x.Name);
                    break;

                default:
                    // Por si no viene nada válido, lo dejas con un orden por defecto
                    queryable = queryable.OrderBy(x => x.Date);
                    break;
            }
        }
        else
        {
            // Orden por defecto si no selecciona nada
            queryable = queryable.OrderBy(x => x.Name);
        }

        if (pagination.Date.HasValue)
        {
            queryable = queryable.Where(x => x.Date.Date == pagination.Date.Value.Date);
        }
        if (pagination.EventTypeId > 0)
        {
            queryable = queryable.Where(x => x.EventTypeId == pagination.EventTypeId);
        }
        return new ActionResponse<IEnumerable<Event>>
        {
            Success = true,
            Result = await queryable
                .Paginate(pagination)
                .ToListAsync()
        };
    }
}