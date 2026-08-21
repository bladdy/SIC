using Microsoft.EntityFrameworkCore.Metadata;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class InvitationEntryUnitOfWork : GenericUnitOfWork<InvitationEntry>, IInvitationEntryUnitOfWork
{
    private readonly IInvitationEntryRepository _invitationEntryRepository;

    public InvitationEntryUnitOfWork(IGenericRepository<InvitationEntry> repository, IInvitationEntryRepository invitationEntryRepository) : base(repository)
    {
        _invitationEntryRepository = invitationEntryRepository;
    }

    public async Task<ActionResponse<InvitationEntry>> AddFullAsync(InvitationEntry invitation) => await _invitationEntryRepository.AddFullAsync(invitation);

    public async Task<ActionResponse<InvitationEntry>> GetByCodeAsync(string code) => await _invitationEntryRepository.GetByCodeAsync(code);

    public async Task<ActionResponse<InvitationEntry>> UpdateFullAsync(InvitationEntry invitation) => await _invitationEntryRepository.UpdateFullAsync(invitation);

    public override async Task<ActionResponse<IEnumerable<InvitationEntry>>> GetAsync(PaginationDTO pagination) => await _invitationEntryRepository.GetAsync(pagination);

    public async Task<ActionResponse<IEnumerable<InvitationEntry>>> GetAllByEventAsync(string eventCode) => await _invitationEntryRepository.GetAllByEventAsync(eventCode);

    public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination) => await _invitationEntryRepository.GetTotalRecordAsync(pagination);
}