using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IInvitationEntryRepository
{
    Task<ActionResponse<InvitationEntry>> GetByCodeAsync(string code);

    Task<ActionResponse<IEnumerable<InvitationEntry>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<IEnumerable<InvitationEntry>>> GetAllByEventAsync(string eventCode);

    Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);

    Task<ActionResponse<InvitationEntry>> AddFullAsync(InvitationEntry invitation);

    Task<ActionResponse<InvitationEntry>> UpdateFullAsync(InvitationEntry invitation);
}