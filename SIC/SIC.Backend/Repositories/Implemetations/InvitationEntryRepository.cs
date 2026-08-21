using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;
using System;
using System.Linq;

namespace SIC.Backend.Repositories.Implemetations
{
    public class InvitationEntryRepository : GenericRepository<InvitationEntry>, IInvitationEntryRepository
    {
        private readonly DataContext _context;

        public InvitationEntryRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ActionResponse<InvitationEntry>> AddFullAsync(InvitationEntry invitation)
        {
            try
            {
                // Buscar la invitación principal
                var existingInvitation = await _context.Invitations
                    .Include(e => e.Event)
                    .FirstOrDefaultAsync(i => i.Code == invitation.Code);

                if (existingInvitation == null)
                {
                    return new ActionResponse<InvitationEntry>
                    {
                        Success = false,
                        Message = "La invitación no existe."
                    };
                }

                // Buscar si ya existe el InvitationEntry
                var existingEntry = await _context.InvitationEntries
                    .FirstOrDefaultAsync(x => x.Code == invitation.Code);

                if (existingEntry != null)
                {
                    // Obtener las propiedades que forman parte de la clave primaria
                    var primaryKeyProperties = _context.Model
                        .FindEntityType(typeof(InvitationEntry))!
                        .FindPrimaryKey()!
                        .Properties;

                    // Actualizar únicamente propiedades que NO sean parte de la clave
                    foreach (var property in _context.Entry(existingEntry).Properties)
                    {
                        if (primaryKeyProperties.Any(x => x.Name == property.Metadata.Name))
                        {
                            continue;
                        }

                        // Evitar actualizar propiedades de navegación/FK
                        // que serán manejadas explícitamente abajo
                        if (property.Metadata.Name == nameof(InvitationEntry.InvitationId) ||
                            property.Metadata.Name == nameof(InvitationEntry.EventId))
                        {
                            continue;
                        }

                        property.CurrentValue = _context
                            .Entry(invitation)
                            .Property(property.Metadata.Name)
                            .CurrentValue;
                    }

                    // Mantener las relaciones correctas
                    existingEntry.Invitation = existingInvitation;
                    existingEntry.Event = existingInvitation.Event!;

                    // Solo asignamos EventId si NO forma parte de la clave
                    var eventIdProperty = _context.Entry(existingEntry)
                        .Property(nameof(InvitationEntry.EventId));

                    if (!primaryKeyProperties.Any(x => x.Name == nameof(InvitationEntry.EventId)))
                    {
                        eventIdProperty.CurrentValue = existingInvitation.Event!.Id;
                    }

                    await _context.SaveChangesAsync();

                    return new ActionResponse<InvitationEntry>
                    {
                        Success = true,
                        Result = existingEntry,
                        Message = "La invitación fue actualizada correctamente."
                    };
                }

                // ==========================================
                // CREAR NUEVO
                // ==========================================

                invitation.Invitation = existingInvitation;
                invitation.Event = existingInvitation.Event!;
                invitation.EventId = existingInvitation.Event!.Id;

                await _context.InvitationEntries.AddAsync(invitation);
                await _context.SaveChangesAsync();

                return new ActionResponse<InvitationEntry>
                {
                    Success = true,
                    Result = invitation,
                    Message = "La invitación fue creada correctamente."
                };
            }
            catch (DbUpdateException exception)
            {
                return new ActionResponse<InvitationEntry>
                {
                    Success = false,
                    Message = exception.InnerException?.Message
                               ?? exception.Message
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<InvitationEntry>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<InvitationEntry>> UpdateFullAsync(InvitationEntry invitation)
        {
            try
            {
                var existingInvitation = _context.Invitations.Include(e => e.Event)
                    .FirstOrDefault(i => i.Code == invitation.Code);
                if (existingInvitation == null)
                {
                    return new ActionResponse<InvitationEntry>
                    {
                        Success = false,
                        Message = "La invitacion no existe."
                    };
                }
                invitation.Invitation = existingInvitation!;
                invitation.Event = existingInvitation?.Event!;
                invitation.EventId = existingInvitation!.Event!.Id;
                _context.Update(invitation);
                await _context.SaveChangesAsync();
                return new ActionResponse<InvitationEntry>
                {
                    Success = true,
                    Result = invitation
                };
            }
            catch (DbUpdateException)
            {
                return new ActionResponse<InvitationEntry>
                {
                    Success = false,
                    Message = "Ya existe un registro con este QR."
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<InvitationEntry>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<InvitationEntry>> GetByCodeAsync(string code)
        {
            var invitationEntry = await _context.InvitationEntries
            .Include(i => i.Invitation)
            .FirstOrDefaultAsync(x => x.Code!.Contains(code));
            if (invitationEntry == null)
            {
                return new ActionResponse<InvitationEntry>
                {
                    Success = true,
                    Message = "Evento no existe."
                };
            }
            return new ActionResponse<InvitationEntry>
            {
                Success = true,
                Result = invitationEntry
            };
        }

        public override async Task<ActionResponse<IEnumerable<InvitationEntry>>> GetAsync(PaginationDTO pagination)
        {
            var queryable = _context.InvitationEntries
                .Include(i => i.Invitation).ThenInclude(t => t.TablesEvents)
                .Include(i => i.Invitation).ThenInclude(i => i!.Guests).ThenInclude(g => g.TablesEvents)
                .Include(e => e.Event).AsQueryable();

            queryable = queryable.Where(x => x.Event!.Code!.ToLower().Contains(pagination!.Code!.ToLower()));

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.ToLower();

                queryable = queryable.Where(x =>
                    x.Invitation!.Name.ToLower().Contains(filter) ||
                    x.Code.ToLower().Contains(filter)
                );
            }
            if (!string.IsNullOrEmpty(pagination.OrderBy))
            {
                switch (pagination.OrderBy)
                {
                    case "HoraAsc":
                        queryable = queryable.OrderBy(x => x.EntryDateTime);
                        break;

                    case "HoraDesc":
                        queryable = queryable.OrderByDescending(x => x.EntryDateTime);
                        break;

                    case "Nombre":
                        queryable = queryable.OrderBy(x => x.Invitation.Name);
                        break;

                    default:
                        // Por si no viene nada válido, lo dejas con un orden por defecto
                        queryable = queryable.OrderBy(x => x.EntryDateTime);
                        break;
                }
            }
            else
            {
                // Orden por defecto si no selecciona nada
                queryable = queryable.OrderBy(x => x.Invitation.Name);
            }

            return new ActionResponse<IEnumerable<InvitationEntry>>
            {
                Success = true,
                Result = await queryable
                    .Paginate(pagination)
                    .ToListAsync()
            };
        }

        public async Task<ActionResponse<IEnumerable<InvitationEntry>>> GetAllByEventAsync(string eventCode)
        {
            var queryable = _context.InvitationEntries
                .Include(i => i.Invitation).ThenInclude(t => t.TablesEvents)
                .Include(i => i.Invitation).ThenInclude(i => i!.Guests).ThenInclude(g => g.TablesEvents)
                .Include(e => e.Event).AsQueryable();

            queryable = queryable.Where(x => x.Event!.Code!.ToLower().Contains(eventCode.ToLower()));

            return new ActionResponse<IEnumerable<InvitationEntry>>
            {
                Success = true,
                Result = await queryable
                    .OrderBy(x => x.Invitation!.Name)
                    .ToListAsync()
            };
        }

        public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
        {
            var queryable = _context.InvitationEntries.Include(i => i.Invitation).AsQueryable();

            queryable = queryable.Where(x => x.Event!.Code!.ToLower().Contains(pagination!.Code!.ToLower()));

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.ToLower();

                queryable = queryable.Where(x =>
                    x.Invitation.Name.ToLower().Contains(filter) ||
                    x.Code.ToLower().Contains(filter)
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
    }
}