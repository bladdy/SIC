using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IInvitationRepository
    {
        Task<ActionResponse<Invitation>> GetByCodeAsync(string code);

        Task<ActionResponse<IEnumerable<Invitation>>> GetAsync();

        Task<ActionResponse<Invitation>> AddFullAsync(Invitation invitation);

        Task<ActionResponse<Invitation>> UpdateFullAsync(Invitation invitation);
    }
}