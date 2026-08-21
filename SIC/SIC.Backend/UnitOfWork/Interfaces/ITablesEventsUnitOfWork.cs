using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface ITablesEventsUnitOfWork
    {
        Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync(int id);

        Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync();

        Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync(PaginationDTO pagination);

        Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination);

        Task<ActionResponse<TablesEvents>> AddFullAsync(CreateOrEditTablesDto createOrEditTablesDto);

        Task<ActionResponse<TablesEvents>> UpdateFullAsync(CreateOrEditTablesDto createOrEditTablesDto);

        Task<ActionResponse<TablesEvents>> AssignTablesAsync(AssignTablesDto tablesDto);

        Task<ActionResponse<GenerateTablesDto>> GenerateTablesAsync(GenerateTablesDto generateTablesDto);

        Task<ActionResponse<bool>> DeleteTablesAsync(int id);

        Task<ActionResponse<bool>> DeleteInvitatonFromTablesAsync(int id);

        Task<ActionResponse<IEnumerable<TablesEvents>>> GetTablesByCodeAsync(string code);

        Task<ActionResponse<InvitationGuest>> AssignGuestTableAsync(AssignGuestTableDto dto);

        Task<ActionResponse<AssignBulkResultDto>> AssignTablesBulkAsync(List<AssignTablesDto> dtos);

        Task<ActionResponse<AssignBulkResultDto>> AssignGuestTableBulkAsync(List<AssignGuestTableDto> dtos);

        Task<ActionResponse<bool>> UnassignGuestFromTableAsync(int guestId);

        Task RecalculateOccupancyAsync(int eventId);
    }
}
