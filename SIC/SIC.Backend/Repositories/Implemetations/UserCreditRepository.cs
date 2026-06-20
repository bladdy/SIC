using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Collections;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implementations
{
    public class UserCreditRepository : IUserCreditRepository
    {
        private readonly DataContext _context;

        public UserCreditRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ActionResponse<UserCreditDTO>> AddAsync(AddCreditsRequest entity)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == entity.UserId);
            if (user == null)
                return new ActionResponse<UserCreditDTO> { Success = false, Message = "Usuario no encontrado." };

            if (string.IsNullOrEmpty(entity.Notes)) entity.Notes = "Recarga de Creditos.";

            var credit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == entity.UserId);

            if (credit == null)
            {
                credit = new UserCredit
                {
                    UserId = entity.UserId,
                    TotalCredits = entity.CreditsToAdd,
                    AvailableCredits = entity.CreditsToAdd,
                    LastUpdated = DateTime.UtcNow,
                    UpdatedBy = entity.UpdatedBy,
                    Notes = entity.Notes
                };

                _context.UserCredits.Add(credit);
                await _context.SaveChangesAsync();
            }
            else
            {
                credit.TotalCredits += entity.CreditsToAdd;
                credit.AvailableCredits += entity.CreditsToAdd;
                credit.LastUpdated = DateTime.UtcNow;
                credit.UpdatedBy = entity.UpdatedBy;
                credit.Notes = entity.Notes;
                await _context.SaveChangesAsync();
            }
            // Guardar historial
            await AddHistoryAsync(credit.Id, "Carga", entity.CreditsToAdd, credit.AvailableCredits, entity.UpdatedBy, entity.Notes);

            return new ActionResponse<UserCreditDTO>
            {
                Success = true,
                Result = new UserCreditDTO
                {
                    Id = credit.Id,
                    UserId = credit.UserId,
                    FullName = user.FullName,
                    TotalCredits = credit.TotalCredits,
                    AvailableCredits = credit.AvailableCredits,
                    ConsumedCredits = credit.ConsumedCredits,
                    PendingCredits = credit.PendingCredits,
                    LastUpdated = credit.LastUpdated,
                    UpdatedBy = credit.UpdatedBy,
                    Notes = credit.Notes
                }
            };
        }

        public async Task AddHistoryAsync(int creditId, string actionType, int amount, int availableAfter, string? modifiedBy, string? notes)
        {
            var history = new UserCreditHistory
            {
                UserCreditId = creditId,
                ActionType = actionType,
                Amount = amount,
                AvailableAfter = availableAfter,
                ModifiedBy = modifiedBy,
                Notes = notes,
                Date = DateTime.UtcNow
            };

            _context.UserCreditHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<ActionResponse<bool>> ConsumeCreditAsync(string userId, string EventName)
        {
            var credit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);

            if (credit == null || credit.AvailableCredits <= 0)
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "No tienes créditos disponibles."
                };

            credit.AvailableCredits -= 1;
            credit.ConsumedCredits += 1;
            credit.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await AddHistoryAsync(credit.Id, "Consumo", -1, credit.AvailableCredits, userId, $"Creación de evento: {EventName}.");

            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }

        public async Task<ActionResponse<IEnumerable<UserCreditHistory>>> GetAsync(PaginationDTO pagination)
        {
            var queryable = _context.UserCreditHistories.Include(e => e.UserCredit).AsQueryable();
            if (!string.IsNullOrWhiteSpace(pagination.UserId))
                queryable = queryable.Where(x => x.UserCredit.UserId == pagination.UserId);

            queryable = queryable.OrderByDescending(x => x.Date);
            return new ActionResponse<IEnumerable<UserCreditHistory>>
            {
                Success = true,
                Result = await queryable
                    .Paginate(pagination)
                    .ToListAsync()
            };
        }

        public async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination)
        {
            var queryable = _context.UserCreditHistories.Include(e => e.UserCredit).AsQueryable();
            if (!string.IsNullOrWhiteSpace(pagination.UserId))
                queryable = queryable.Where(x => x.UserCredit.UserId == pagination.UserId);

            double count = await queryable.CountAsync();
            int totalPages = (int)Math.Ceiling(count / pagination.PageSize);
            return new ActionResponse<int>
            {
                Success = true,
                Result = totalPages
            };
        }

        public async Task<ActionResponse<UserCreditDTO>> GetByUserIdAsync(string userId)
        {
            var credit = await _context.UserCredits.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

            if (credit == null)
                return new ActionResponse<UserCreditDTO>
                { Success = false, Message = "El usuario no tiene créditos asignados." };
            var creditDto = new UserCreditDTO
            {
                Id = credit.Id,
                UserId = credit.UserId,
                FullName = credit.User.FullName,
                TotalCredits = credit.TotalCredits,
                AvailableCredits = credit.AvailableCredits,
                ConsumedCredits = credit.ConsumedCredits,
                PendingCredits = credit.PendingCredits,
                LastUpdated = credit.LastUpdated,
                UpdatedBy = credit.UpdatedBy,
                Notes = credit.Notes
            };

            return new ActionResponse<UserCreditDTO>
            {
                Success = true,
                Result = creditDto
            };
        }

        public async Task<ActionResponse<IEnumerable<UserCreditHistoryDTO>>> GetHistoryAsync(string userId)
        {
            var credit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
            if (credit == null)

                return new ActionResponse<IEnumerable<UserCreditHistoryDTO>>
                {
                    Success = false,
                    Message = "No existe historial para este usuario."
                };

            var history = await _context.UserCreditHistories
                .Where(h => h.UserCreditId == credit.Id)
                .OrderByDescending(h => h.Date)
                .ToListAsync();

            var result = history.Select(h => new UserCreditHistoryDTO
            {
                ActionType = h.ActionType,
                Amount = h.Amount,
                AvailableAfter = h.AvailableAfter,
                Date = h.Date,
                ModifiedBy = h.ModifiedBy,
                Notes = h.Notes
            }).ToList();
            return new ActionResponse<IEnumerable<UserCreditHistoryDTO>>
            {
                Success = true,
                Result = result
            };
        }

        public async Task<ActionResponse<IEnumerable<UserCreditDTO>>> GetPlannersWithCreditsAsync()
        {
            var credits = await _context.UserCredits.Include(x => x.User).ToListAsync();
            var result = credits.Select(c => new UserCreditDTO
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User.FullName,
                TotalCredits = c.TotalCredits,
                AvailableCredits = c.AvailableCredits,
                ConsumedCredits = c.ConsumedCredits,
                PendingCredits = c.PendingCredits,
                LastUpdated = c.LastUpdated,
                UpdatedBy = c.UpdatedBy,
                Notes = c.Notes
            }).ToList();

            return new ActionResponse<IEnumerable<UserCreditDTO>>
            {
                Success = true,
                Result = result
            };
        }

        public async Task<ActionResponse<StripeEventLog>> AddStripeEventLogAsync(StripeEventLog entity)
        {
            _context.StripeEventLogs.Add(entity);
            await _context.SaveChangesAsync();
            return new ActionResponse<StripeEventLog>
            {
                Success = true,
                Result = entity
            };
        }

        public async Task<ActionResponse<bool>> ExistStripeEventLogAsync(string id)
        {
            var alreadyProcessed = await _context.StripeEventLogs
            .FirstOrDefaultAsync(x => x.EventId == id);

            if (alreadyProcessed == null)
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "No hay logs."
                };

            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }
    }
}