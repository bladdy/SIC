using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IInvitationUnitOfWork
    {
        Task<ActionResponse<Invitation>> GetByCodeAsync(string code);

        Task<ActionResponse<IEnumerable<Invitation>>> GetAsync();

        Task<ActionResponse<IEnumerable<Invitation>>> GetAsync(PaginationDTO pagination);

        Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);

        Task<ActionResponse<Invitation>> AddFullAsync(Invitation invitation);

        Task<ActionResponse<Invitation>> UpdateFullAsync(Invitation invitation);
    }
}