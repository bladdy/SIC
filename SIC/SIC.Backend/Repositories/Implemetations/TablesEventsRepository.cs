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
            var updateTable = await _context.TablesEvents
                .Include(e => e.Event)
                .Include(i => i.Invitations)
                    .ThenInclude(g => g.Guests)
                .FirstOrDefaultAsync(x => x.Id == createOrEditTablesDto.Id);

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
                .Include(t => t.Invitations)
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

            if (table.Invitations.Any(i => i.Id == invitation.Id))
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

            table.Invitations.Add(invitation);

            _context.TablesEvents.Update(table);

            await _context.SaveChangesAsync();

            await RecalculateOccupancyAsync(invitation.EventId);

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
                    OccupiedSeats = 0
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
            var entities = await _context.TablesEvents
                .Include(e => e.Event)
                .Include(i => i.Invitations)
                    .ThenInclude(g => g.Guests)
                        .ThenInclude(g => g.TablesEvents)
                .Include(t => t.Guests)
                    .ThenInclude(g => g.Invitation)
                .Where(e => e.EventId == id)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<TablesEvents>>
            {
                Success = true,
                Result = entities
            };
        }

        public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
        {
            var queryable = _context.TablesEvents
                .Include(t => t.Invitations)
                    .ThenInclude(i => i.Guests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.ToLower();

                queryable = queryable.Where(t =>
                    t.Name.ToLower().Contains(filter)

                    || t.Invitations.Any(i =>
                        i.Name.ToLower().Contains(filter))

                    || t.Invitations.Any(i =>
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
                .Include(t => t.Invitations)
                    .ThenInclude(i => i.Guests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.ToLower();

                queryable = queryable.Where(t =>
                    t.Name.ToLower().Contains(filter)

                    || t.Invitations.Any(i =>
                        i.Name.ToLower().Contains(filter))

                    || t.Invitations.Any(i =>
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
                .Include(t => t.Invitations)
                    .ThenInclude(i => i.Guests)
                        .ThenInclude(g => g.TablesEvents)
                .Include(t => t.Guests)
                    .ThenInclude(g => g.Invitation)
                .ToListAsync();
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

                var eventId = invitation.EventId;

                invitation.TablesEvents = null;
                invitation.TablesEventsId = null;

                await _context.SaveChangesAsync();

                await RecalculateOccupancyAsync(eventId);

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
                .Include(t => t.Invitations)
                .Include(t => t.Guests)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "La mesa no existe."
                };
            }

            foreach (var invitation in table.Invitations)
            {
                invitation.TablesEventsId = null;
            }

            foreach (var guest in table.Guests)
            {
                guest.TablesEventsId = null;
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
            var entities = await _context.TablesEvents
                .Include(i => i.Invitations)
                    .ThenInclude(g => g.Guests)
                .Include(t => t.Guests)
                    .ThenInclude(g => g.Invitation)
                .Where(e => e.Event!.Code == code)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<TablesEvents>>
            {
                Success = true,
                Result = entities
            };
        }

        public async Task<ActionResponse<AssignBulkResultDto>> AssignTablesBulkAsync(List<AssignTablesDto> dtos)
        {
            try
            {
                if (dtos == null || dtos.Count == 0)
                {
                    return new ActionResponse<AssignBulkResultDto>
                    {
                        Success = false,
                        Message = "No hay invitaciones seleccionadas."
                    };
                }

                var tableId = dtos.First().TableId;

                var table = await _context.TablesEvents
                    .Include(t => t.Invitations)
                    .FirstOrDefaultAsync(t => t.Id == tableId);

                if (table == null)
                {
                    return new ActionResponse<AssignBulkResultDto>
                    {
                        Success = false,
                        Message = "La mesa no existe."
                    };
                }

                var invitationIds = dtos.Select(d => d.InvitationId).Distinct().ToList();
                var invitations = await _context.Invitations
                    .Include(i => i.Guests)
                    .Where(i => invitationIds.Contains(i.Id))
                    .ToListAsync();

                var result = new AssignBulkResultDto();
                var pending = new List<Invitation>();

                foreach (var invitationId in invitationIds)
                {
                    var invitation = invitations.FirstOrDefault(i => i.Id == invitationId);
                    if (invitation == null)
                    {
                        result.Skipped.Add($"La invitación {invitationId} no existe.");
                        continue;
                    }
                    if (table.Invitations.Any(i => i.Id == invitation.Id))
                    {
                        result.Skipped.Add($"'{invitation.Name}' ya está asignada a esta mesa.");
                        continue;
                    }
                    if (invitation.Guests.Count(g => g.Status == Status.Attend) == 0)
                    {
                        result.Skipped.Add($"'{invitation.Name}' aun no ha confirmado asistencia.");
                        continue;
                    }
                    pending.Add(invitation);
                }

                var totalGuests = pending.Sum(i => i.Guests.Count(g => g.Status == Status.Attend));
                var available = table.Seats - table.OccupiedSeats;
                if (totalGuests > available)
                {
                    return new ActionResponse<AssignBulkResultDto>
                    {
                        Success = false,
                        Message = $"La mesa tiene {table.Seats} lugares y solo quedan {available} disponibles (se requieren {totalGuests})."
                    };
                }

                foreach (var invitation in pending)
                {
                    table.Invitations.Add(invitation);
                    result.Assigned++;
                }

                await _context.SaveChangesAsync();
                await RecalculateOccupancyAsync(table.EventId);

                return new ActionResponse<AssignBulkResultDto>
                {
                    Success = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<AssignBulkResultDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<AssignBulkResultDto>> AssignGuestTableBulkAsync(List<AssignGuestTableDto> dtos)
        {
            try
            {
                if (dtos == null || dtos.Count == 0)
                {
                    return new ActionResponse<AssignBulkResultDto>
                    {
                        Success = false,
                        Message = "No hay invitados seleccionados."
                    };
                }

                var tableId = dtos.First().TablesEventsId;
                if (!tableId.HasValue)
                {
                    return new ActionResponse<AssignBulkResultDto>
                    {
                        Success = false,
                        Message = "La mesa no es válida."
                    };
                }

                var table = await _context.TablesEvents
                    .FirstOrDefaultAsync(t => t.Id == tableId.Value);

                if (table == null)
                {
                    return new ActionResponse<AssignBulkResultDto>
                    {
                        Success = false,
                        Message = "La mesa no existe."
                    };
                }

                var guestIds = dtos.Select(d => d.GuestId).Distinct().ToList();
                var guests = await _context.InvitationGuest
                    .Include(g => g.Invitation)
                    .Where(g => guestIds.Contains(g.Id))
                    .ToListAsync();

                var result = new AssignBulkResultDto();

                foreach (var guest in guests)
                {
                    if (guest.Invitation == null || guest.Invitation.EventId != table.EventId)
                    {
                        result.Skipped.Add($"'{guest.GuestName}' no pertenece a este evento.");
                        continue;
                    }
                    guest.TablesEventsId = table.Id;
                    result.Assigned++;
                }

                await _context.SaveChangesAsync();
                await RecalculateOccupancyAsync(table.EventId);

                return new ActionResponse<AssignBulkResultDto>
                {
                    Success = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<AssignBulkResultDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<InvitationGuest>> AssignGuestTableAsync(AssignGuestTableDto dto)
        {
            try
            {
                var guest = await _context.InvitationGuest
                    .Include(g => g.Invitation)
                    .FirstOrDefaultAsync(g => g.Id == dto.GuestId);

                if (guest == null)
                {
                    return new ActionResponse<InvitationGuest>
                    {
                        Success = false,
                        Message = "El invitado no existe."
                    };
                }

                if (dto.TablesEventsId.HasValue)
                {
                    var table = await _context.TablesEvents
                        .FirstOrDefaultAsync(t => t.Id == dto.TablesEventsId.Value);

                    if (table == null)
                    {
                        return new ActionResponse<InvitationGuest>
                        {
                            Success = false,
                            Message = "La mesa no existe."
                        };
                    }

                    if (table.EventId != guest.Invitation!.EventId)
                    {
                        return new ActionResponse<InvitationGuest>
                        {
                            Success = false,
                            Message = "La mesa no pertenece al mismo evento que la invitación."
                        };
                    }
                }

                guest.TablesEventsId = dto.TablesEventsId;

                await _context.SaveChangesAsync();

                await RecalculateOccupancyAsync(guest.Invitation!.EventId);

                return new ActionResponse<InvitationGuest>
                {
                    Success = true,
                    Result = guest
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<InvitationGuest>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<bool>> UnassignGuestFromTableAsync(int guestId)
        {
            try
            {
                var guest = await _context.InvitationGuest
                    .Include(g => g.Invitation)
                    .FirstOrDefaultAsync(g => g.Id == guestId);

                if (guest == null)
                {
                    return new ActionResponse<bool>
                    {
                        Success = false,
                        Message = "El invitado no existe."
                    };
                }

                if (!guest.TablesEventsId.HasValue)
                {
                    return new ActionResponse<bool>
                    {
                        Success = false,
                        Message = "El invitado no tiene mesa individual."
                    };
                }

                var eventId = guest.Invitation!.EventId;
                guest.TablesEventsId = null;
                await _context.SaveChangesAsync();
                await RecalculateOccupancyAsync(eventId);

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

        public async Task RecalculateOccupancyAsync(int eventId)
        {
            var tables = await _context.TablesEvents
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            if (tables.Count == 0) return;

            var inheritedCounts = await _context.InvitationGuest
                .Where(g => g.Status == Status.Attend
                         && g.TablesEventsId == null
                         && g.Invitation!.EventId == eventId
                         && g.Invitation!.TablesEventsId != null)
                .GroupBy(g => g.Invitation!.TablesEventsId!.Value)
                .Select(grp => new { TableId = grp.Key, Count = grp.Count() })
                .ToListAsync();

            var directCounts = await _context.InvitationGuest
                .Where(g => g.TablesEventsId != null
                         && g.Invitation!.EventId == eventId
                         && g.Status == Status.Attend)
                .GroupBy(g => g.TablesEventsId)
                .Select(g => new { TableId = g.Key, Count = g.Count() })
                .ToListAsync();

            var inheritedDict = inheritedCounts.ToDictionary(x => x.TableId, x => x.Count);
            var directDict = directCounts
                .Where(x => x.TableId.HasValue)
                .ToDictionary(x => x.TableId!.Value, x => x.Count);

            foreach (var table in tables)
            {
                table.OccupiedSeats =
                    (inheritedDict.TryGetValue(table.Id, out var inherited) ? inherited : 0) +
                    (directDict.TryGetValue(table.Id, out var direct) ? direct : 0);
            }

            await _context.SaveChangesAsync();
        }
    }
}
