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
                _context.Add(invitation);
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

        public override async Task<ActionResponse<IEnumerable<InvitationEntry>>> GetAsync(PaginationDTO pagination)
        {
            var queryable = _context.InvitationEntries.Include(i => i.Invitation).Include(e => e.Event).AsQueryable();

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