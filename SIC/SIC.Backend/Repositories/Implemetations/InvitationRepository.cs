using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;
using System;
using System.Linq;

namespace SIC.Backend.Repositories.Implemetations
{
    public class InvitationRepository : GenericRepository<Invitation>, IInvitationRepository
    {
        private readonly DataContext _context;

        public InvitationRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ActionResponse<Invitation>> GetByCodeAsync(string code)
        {
            var invitations = await _context.Invitations.Include(e => e.Event).ThenInclude(e => e!.EventType).Include(g => g.Guests).
                FirstOrDefaultAsync(x => x.Code == code);
            if (invitations == null)
            {
                return new ActionResponse<Invitation>
                {
                    Success = true,
                    Message = "La invitacion no existe."
                };
            }
            return new ActionResponse<Invitation>
            {
                Success = true,
                Result = invitations
            };
        }

        public override async Task<ActionResponse<IEnumerable<Invitation>>> GetAsync(PaginationDTO pagination)
        {
            //ToDo: Agregar el filtro para que filte cada uno de los Guests
            var queryable = _context.Invitations.Include(t => t.TemplateSents).Include(g => g.Guests).AsQueryable();
            queryable = queryable.Where(x => x.EventId == pagination.Id);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }
            return new ActionResponse<IEnumerable<Invitation>>
            {
                Success = true,
                Result = await queryable
                    .OrderBy(x => x.Name)
                    .Paginate(pagination)
                    .ToListAsync()
            };
        }

        public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
        {
            // ToDo: Agregar el filtro para que filte cada uno de los Guests
            var queryable = _context.Invitations.AsQueryable();
            queryable = queryable.Where(x => x.EventId == pagination.Id);
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }
            double count = await queryable.CountAsync();
            int totalPages = (int)Math.Ceiling(count / pagination.PageSize);
            return new ActionResponse<int>
            {
                Success = true,
                Result = totalPages
            };
        }

        public override async Task<ActionResponse<IEnumerable<Invitation>>> GetAsync()
        {
            var invitations = await _context.Invitations.ToListAsync();
            return new ActionResponse<IEnumerable<Invitation>>
            {
                Success = true,
                Result = invitations
            };
        }

        public async Task<ActionResponse<Invitation>> AddFullAsync(Invitation invitation)
        {
            try
            {
                invitation.Code = CodeGenerator.GenerateCode();
                var Event = await _context.Events.FirstOrDefaultAsync(x => x.Id == invitation.EventId);
                if (Event == null)
                {
                    return new ActionResponse<Invitation>
                    {
                        Success = false,
                        Message = "El evento no existe"
                    };
                }
                invitation.Event = Event;
                _context.Add(invitation);
                await _context.SaveChangesAsync();
                return new ActionResponse<Invitation>
                {
                    Success = true,
                    Result = invitation
                };
            }
            catch (DbUpdateException)
            {
                return new ActionResponse<Invitation>
                {
                    Success = false,
                    Message = "Ya existe esta invitacion"
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<Invitation>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<Invitation>> UpdateFullAsync(Invitation invitation)
        {
            try
            {
                // 1. Cargar la invitación actual con sus invitados desde la BD
                var currentInvitation = await _context.Invitations
                    .Include(i => i.Guests)
                    .FirstOrDefaultAsync(i => i.Id == invitation.Id);

                if (currentInvitation == null)
                {
                    return new ActionResponse<Invitation>
                    {
                        Success = false,
                        Message = "La invitación no existe."
                    };
                }

                // 2. Actualizar valores simples de Invitation
                _context.Entry(currentInvitation).CurrentValues.SetValues(invitation);

                // ==== 3. Sincronizar la colección de Guests ====

                // IDs existentes en la BD
                var dbGuests = currentInvitation.Guests.ToList();

                // IDs enviados desde la UI (pueden ser 0 si son nuevos)
                var incomingGuests = invitation.Guests.ToList();

                // 🔥 3.1 ELIMINAR GUESTS QUE FUERON REMOVIDOS EN LA UI
                foreach (var guestInDb in dbGuests)
                {
                    if (!incomingGuests.Any(g => g.Id == guestInDb.Id))
                    {
                        _context.InvitationGuest.Remove(guestInDb);
                    }
                }

                // 🔥 3.2 ACTUALIZAR Y AGREGAR GUESTS NUEVOS
                foreach (var incoming in incomingGuests)
                {
                    var existing = dbGuests.FirstOrDefault(g => g.Id == incoming.Id);

                    if (existing == null)
                    {
                        // 👉 ES NUEVO
                        incoming.InvitationId = invitation.Id;
                        _context.InvitationGuest.Add(incoming);
                    }
                    else
                    {
                        // 👉 ES EXISTENTE — actualizar campos
                        _context.Entry(existing).CurrentValues.SetValues(incoming);
                    }
                }

                // 4. Guardar todos los cambios
                await _context.SaveChangesAsync();

                return new ActionResponse<Invitation>
                {
                    Success = true,
                    Result = invitation
                };
            }
            catch (DbUpdateException)
            {
                return new ActionResponse<Invitation>
                {
                    Success = false,
                    Message = "Ya existe esta invitación."
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<Invitation>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }
        public async Task<ActionResponse<IEnumerable<Invitation>>> GetAllAsync(string code)
        {
            var invitations = await _context.Invitations.Include(e => e.Event).ThenInclude(e => e!.EventType).Where(x => x.Event!.Code == code && x.Status == Status.Attend).ToListAsync();
            if (invitations == null)
            {
                return new ActionResponse<IEnumerable<Invitation>>
                {
                    Success = true,
                    Message = "Evento no existe."
                };
            }

            return new ActionResponse<IEnumerable<Invitation>>
            {
                Success = true,
                Result = invitations
            };
        }
        public async Task<ActionResponse<IEnumerable<Invitation>>> GetInivtationsByyEventIdAsync(int EventId)
        {
            var invitations = await _context.Invitations.Include(e => e.Event).ThenInclude(e => e!.EventType).Where(x => x.EventId == EventId).ToListAsync();
            if (invitations == null)
            {
                return new ActionResponse<IEnumerable<Invitation>>
                {
                    Success = true,
                    Message = "Evento no existe."
                };
            }

            return new ActionResponse<IEnumerable<Invitation>>
            {
                Success = true,
                Result = invitations
            };
        }

        public async Task<ActionResponse<InvitationConfirmationDto>> UpdateForConfirmarionFullAsync(InvitationConfirmationDto confirmationDto)
        {
            try
            {
                var invitations = await _context.Invitations.FirstOrDefaultAsync(x => x.Code == confirmationDto.CodigoInvitacion);
                if (invitations == null)
                {
                    return new ActionResponse<InvitationConfirmationDto>
                    {
                        Success = false,
                        Message = "La invitacion no existe."
                    };
                }
                invitations.NumberConfirmedAdults = confirmationDto.ConfirmadosAdultos;
                invitations.NumberConfirmedYouths = confirmationDto.confirmadosJovenes;
                invitations.NumberConfirmedChildren = confirmationDto.ConfirmadosMenores;
                invitations.ConfirmationDate = DateTime.Now;
                invitations.Comments = confirmationDto.Mensaje;
                if (confirmationDto.ConfirmacionAsistencia)
                {
                    invitations.Status = Shared.Enums.Status.Attend;
                }
                else
                {
                    invitations.Status = Shared.Enums.Status.NotAttend;
                }

                _context.Update(invitations);
                await _context.SaveChangesAsync();
                return new ActionResponse<InvitationConfirmationDto>
                {
                    Success = true,
                    Result = confirmationDto
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<InvitationConfirmationDto>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<bool>> DeleteAsync(Invitation invitation)
        {
            _context.Remove(invitation);
            await _context.SaveChangesAsync(); // Ensure changes are saved to the database

            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }

        public async Task<ActionResponse<bool>> DeleteByIdAsync(int id)
        {
            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "La invitacion no existe."
                };
            }
            _context.Remove(invitation);
            await _context.SaveChangesAsync(); // Ensure changes are saved to the database

            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }

        public async Task<ActionResponse<ResponseInvitationDTO>> UpdateForConfirmationListFullAsync(ResponseInvitationDTO invitation)
        {
            try
            {
                var invitations = await _context.Invitations.FirstOrDefaultAsync(x => x.Code == invitation.Code);
                if (invitations == null)
                {
                    return new ActionResponse<ResponseInvitationDTO>
                    {
                        Success = false,
                        Message = "La invitación no existe."
                    };
                }

                // Optimización: convierto la lista de invitados en un diccionario
                var dbGuestsDict = invitations.Guests.ToDictionary(g => g.Id);

                // Actualizar los contadores de confirmación
                invitations.NumberConfirmedAdults = invitation.Guests.Count(T => T.GuestType == 1 && T.Status == 19);
                invitations.NumberConfirmedYouths = invitation.Guests.Count(T => T.GuestType == 2 && T.Status == 19);
                invitations.NumberConfirmedChildren = invitation.Guests.Count(T => T.GuestType == 3 && T.Status == 19);
                invitations.ConfirmationDate = DateTime.Now;
                invitations.Comments = invitation.Comments;

                // Actualizar invitados existentes
                foreach (var incoming in invitation.Guests)
                {
                    if (dbGuestsDict.TryGetValue(incoming.Id, out var existing))
                    {
                        _context.Entry(existing).CurrentValues.SetValues(incoming);
                    }
                }

                invitations.Status = (Shared.Enums.Status)invitation.Status;

                // Guardar cambios en la invitación
                _context.Update(invitations);
                await _context.SaveChangesAsync();

                return new ActionResponse<ResponseInvitationDTO>
                {
                    Success = true,
                    Result = invitation
                };
            }
            catch (Exception exception)
            {
                // Puede ser útil registrar el error para diagnóstico
                return new ActionResponse<ResponseInvitationDTO>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        
    }
}