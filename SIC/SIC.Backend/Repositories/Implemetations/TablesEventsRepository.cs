using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations
{
    public class TablesEventsRepository : GenericRepository<TablesEvents>, ITablesEventsRepository
    {
        private readonly DataContext _context;

        public TablesEventsRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ActionResponse<TablesEvents>> AddFullAsync(CreateOrEditTablesDto createOrEditTablesDto)
        {
            var events = await _context.Events.FirstOrDefaultAsync(x => x.Id == createOrEditTablesDto.EventoId);
            var tablesEvents = await _context.TablesEvents.Where(x => x.EventId == createOrEditTablesDto.EventoId).ToListAsync();
            if (events == null)
            {
                return new ActionResponse<TablesEvents>
                {
                    Success = false,
                    Message = "El Evento no es valido."
                };
            }
            var newTable = new TablesEvents
            {
                Event = events,
                Number = tablesEvents.Count,
                Name = createOrEditTablesDto.Name,
                Invitation = new List<Invitation>(),
                Description = createOrEditTablesDto.Description,
                Seats = createOrEditTablesDto.Seats,
                OccupiedSeats = 0
            };
            _context.Add(newTable);
            await _context.SaveChangesAsync();
            return new ActionResponse<TablesEvents>
            {
                Success = true,
                Result = newTable,
            };
        }

        public async Task<ActionResponse<TablesEvents>> UpdateFullAsync(CreateOrEditTablesDto createOrEditTablesDto)
        {
            var updateTable = await _context.TablesEvents.Include(e => e.Event).Include(i => i.Invitation).FirstOrDefaultAsync(x => x.Id == createOrEditTablesDto.Id);

            if (updateTable == null)
            {
                return new ActionResponse<TablesEvents>
                {
                    Success = false,
                    Message = "La mesa no existe."
                };
            }
            updateTable.Seats = createOrEditTablesDto.Seats;
            updateTable.Description = createOrEditTablesDto.Description;
            updateTable.Name = createOrEditTablesDto.Name;

            _context.Update(updateTable);
            await _context.SaveChangesAsync();

            return new ActionResponse<TablesEvents>
            {
                Success = true,
                Result = updateTable,
            };
        }

        public async Task<ActionResponse<TablesEvents>> AssignTablesAsync(AssignTablesDto tablesDto)
        {
            var table = await _context.TablesEvents
                .Include(t => t.Invitation)
                .FirstOrDefaultAsync(t => t.Id == tablesDto.TableId);

            if (table == null)
            {
                return new ActionResponse<TablesEvents>
                {
                    Success = false,
                    Message = "La mesa no existe."
                };
            }

            var invitation = await _context.Invitations
                .Include(i => i.Guests)
                .FirstOrDefaultAsync(i => i.Id == tablesDto.InvitationId);

            if (invitation == null)
            {
                return new ActionResponse<TablesEvents>
                {
                    Success = false,
                    Message = "La invitación no existe."
                };
            }

            // Validar que no esté asignada ya
            if (table.Invitation.Any(i => i.Id == invitation.Id))
            {
                return new ActionResponse<TablesEvents>
                {
                    Success = false,
                    Message = "La invitación ya está asignada a esta mesa."
                };
            }

            var confirmedGuests = invitation.Guests
                .Count(g => g.Status == Status.Attend);

            if (confirmedGuests == 0)
            {
                return new ActionResponse<TablesEvents>
                {
                    Success = false,
                    Message = "El invitado aun no ha confirmado asistencia"
                };
            }

            var occupiedSeats = table.OccupiedSeats + confirmedGuests;

            if (occupiedSeats > table.Seats)
            {
                return new ActionResponse<TablesEvents>
                {
                    Success = false,
                    Message = $"La mesa tiene {table.Seats} lugares y solo quedan {table.Seats - table.OccupiedSeats} disponibles."
                };
            }

            // Asignar invitación a la mesa
            table.Invitation.Add(invitation);

            // Actualizar ocupación
            table.OccupiedSeats = occupiedSeats;

            _context.TablesEvents.Update(table);

            await _context.SaveChangesAsync();

            return new ActionResponse<TablesEvents>
            {
                Success = true,
                Result = table
            };
        }

        public async Task<ActionResponse<GenerateTablesDto>> GenerateTablesAsync(GenerateTablesDto generateTablesDto)
        {
            var evento = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == generateTablesDto.EventoId);

            if (evento == null)
            {
                return new ActionResponse<GenerateTablesDto>
                {
                    Success = false,
                    Message = "El evento no existe."
                };
            }

            if (generateTablesDto.NumbersTables <= 0)
            {
                return new ActionResponse<GenerateTablesDto>
                {
                    Success = false,
                    Message = "Debe indicar al menos una mesa."
                };
            }

            if (generateTablesDto.NumberOfSeats <= 0)
            {
                return new ActionResponse<GenerateTablesDto>
                {
                    Success = false,
                    Message = "Debe indicar la cantidad de asientos por mesa."
                };
            }

            var existingTables = await _context.TablesEvents
                .AnyAsync(t => t.EventId == generateTablesDto.EventoId);
            /*
            if (existingTables)
            {
                return new ActionResponse<GenerateTablesDto>
                {
                    Success = false,
                    Message = "Ya existen mesas generadas para este evento."
                };
            }*/

            var tables = new List<TablesEvents>();

            for (int i = 1; i <= generateTablesDto.NumbersTables; i++)
            {
                tables.Add(new TablesEvents
                {
                    EventId = generateTablesDto.EventoId,
                    Number = i,
                    Name = $"{i}",
                    Description = $"{i}",
                    Seats = generateTablesDto.NumberOfSeats,
                    OccupiedSeats = 0,
                    Invitation = new List<Invitation>()
                });
            }

            await _context.TablesEvents.AddRangeAsync(tables);
            await _context.SaveChangesAsync();

            return new ActionResponse<GenerateTablesDto>
            {
                Success = true,
                Message = $"{tables.Count} mesas generadas correctamente.",
                Result = generateTablesDto
            };
        }

        public async Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync(int id)
        {
            var entities = await _context.TablesEvents.Include(e => e.Event).Include(i => i.Invitation).ThenInclude(g => g.Guests).Where(e => e.EventId == id).ToListAsync();

            return new ActionResponse<IEnumerable<TablesEvents>>
            {
                Success = true,
                Result = entities
            };
        }

        public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
        {
            var queryable = _context.TablesEvents
                .Include(t => t.Invitation)
                    .ThenInclude(i => i.Guests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.ToLower();

                queryable = queryable.Where(t =>
                    t.Name.ToLower().Contains(filter)

                    || t.Invitation.Any(i =>
                        i.Name.ToLower().Contains(filter))

                    || t.Invitation.Any(i =>
                        i.Guests.Any(g =>
                            g.GuestName!.ToLower().Contains(filter)))
                );
            }
            double count = await queryable.CountAsync();
            int totalPages = (int)Math.Ceiling(count / pagination.PageSize);
            return new ActionResponse<int>
            {
                Success = true,
                Result = totalPages
            };
        }

        public override async Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync(PaginationDTO pagination)
        {
            var queryable = _context.TablesEvents
                .Include(t => t.Invitation)
                    .ThenInclude(i => i.Guests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.ToLower();

                queryable = queryable.Where(t =>
                    t.Name.ToLower().Contains(filter)

                    || t.Invitation.Any(i =>
                        i.Name.ToLower().Contains(filter))

                    || t.Invitation.Any(i =>
                        i.Guests.Any(g =>
                            g.GuestName!.ToLower().Contains(filter)))
                );
            }

            return new ActionResponse<IEnumerable<TablesEvents>>
            {
                Success = true,
                Result = await queryable
                    .Paginate(pagination)
                    .ToListAsync()
            };
        }

        public override async Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync()
        {
            var queryable = await _context.TablesEvents
                .Include(t => t.Invitation)
                    .ThenInclude(i => i.Guests).ToListAsync();
            return new ActionResponse<IEnumerable<TablesEvents>>
            {
                Success = true,
                Result = queryable
            };
        }

        public async Task<ActionResponse<bool>> DeleteInvitatonFromTablesAsync(int id)
        {
            try
            {
                var invitation = await _context.Invitations
                    .Include(i => i.Guests)
                    .Include(i => i.TablesEvents)
                    .FirstOrDefaultAsync(i => i.TablesEvents != null && i.Id == id);

                if (invitation == null)
                {
                    return new ActionResponse<bool>
                    {
                        Success = false,
                        Message = "No se encontró la invitación asignada."
                    };
                }

                var table = invitation.TablesEvents;

                var confirmedGuests = invitation.Guests?
                    .Count(g => g.Status == Status.Attend) ?? 0;

                if (table != null)
                {
                    table.OccupiedSeats -= confirmedGuests;

                    if (table.OccupiedSeats < 0)
                    {
                        table.OccupiedSeats = 0;
                    }
                }

                invitation.TablesEvents = null;
                invitation.TablesEventsId = null;

                await _context.SaveChangesAsync();

                return new ActionResponse<bool>
                {
                    Success = true,
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<bool>> DeleteTablesAsync(int id)
        {
            var table = await _context.TablesEvents
                .Include(t => t.Invitation)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "La mesa no existe."
                };
            }

            // Desasignar invitaciones de la mesa
            foreach (var invitation in table.Invitation)
            {
                invitation.TablesEventsId = null;
            }

            _context.TablesEvents.Remove(table);

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                Success = true,
                Message = $"La mesa '{table.Name}' fue eliminada correctamente."
            };
        }

        public async Task<ActionResponse<IEnumerable<TablesEvents>>> GetTablesByCodeAsync(string code)
        {
            var entities = await _context.TablesEvents.Include(e => e.Event).Include(i => i.Invitation).ThenInclude(g => g.Guests).Where(e => e.Event!.Code == code).ToListAsync();

            return new ActionResponse<IEnumerable<TablesEvents>>
            {
                Success = true,
                Result = entities
            };
        }
    }
}