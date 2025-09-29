using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

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
            var invitations = await _context.Invitations.Include(e => e.Event).ThenInclude(e => e.EventType).FirstOrDefaultAsync(x => x.Code!.Contains(code));
            if (invitations == null)
            {
                return new ActionResponse<Invitation>
                {
                    Success = true,
                    Message = "Evento no existe."
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
            var queryable = _context.Invitations.AsQueryable();
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
                _context.Update(invitation);
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
    }
}