using Microsoft.Extensions.Logging;
using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class InvitationUnitOfWork : GenericUnitOfWork<Invitation>, IInvitationUnitOfWork
    {
        private readonly IInvitationRepository _invitationstRepository;

        public InvitationUnitOfWork(IGenericRepository<Invitation> repository, IInvitationRepository invitationstRepository) : base(repository)
        {
            _invitationstRepository = invitationstRepository;
        }

        public async Task<ActionResponse<Invitation>> GetByCodeAsync(string code) => await _invitationstRepository.GetByCodeAsync(code);

        public override async Task<ActionResponse<IEnumerable<Invitation>>> GetAsync() => await _invitationstRepository.GetAsync();

        public async Task<ActionResponse<Invitation>> AddFullAsync(Invitation invitation) => await _invitationstRepository.AddFullAsync(invitation);

        public async Task<ActionResponse<Invitation>> UpdateFullAsync(Invitation invitation) => await _invitationstRepository.UpdateFullAsync(invitation);
    }
}