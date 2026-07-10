using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class TablesEventsUnitOfWork : GenericUnitOfWork<TablesEvents>, ITablesEventsUnitOfWork
    {
        //TablesEventsRepository ITablesEventsRepository
        private readonly ITablesEventsRepository _tablesEventsRepository;

        public TablesEventsUnitOfWork(ITablesEventsRepository tablesEventsRepository, IGenericRepository<TablesEvents> repository) : base(repository)
        {
            _tablesEventsRepository = tablesEventsRepository;
        }

        public async Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync(int id) => await _tablesEventsRepository.GetAsync(id);

        public override async Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync() => await _tablesEventsRepository.GetAsync();

        public override async Task<ActionResponse<IEnumerable<TablesEvents>>> GetAsync(PaginationDTO pagination) => await _tablesEventsRepository.GetAsync(pagination);

        public override async Task<ActionResponse<int>> GetTotalRecordAsync(PaginationDTO pagination) =>
            await _tablesEventsRepository.GetTotalRecordAsync(pagination);

        public async Task<ActionResponse<TablesEvents>> AddFullAsync(CreateOrEditTablesDto createOrEditTablesDto) => await _tablesEventsRepository.AddFullAsync(createOrEditTablesDto);

        public async Task<ActionResponse<TablesEvents>> UpdateFullAsync(CreateOrEditTablesDto createOrEditTablesDto) => await _tablesEventsRepository.UpdateFullAsync(createOrEditTablesDto);

        public async Task<ActionResponse<TablesEvents>> AssignTablesAsync(AssignTablesDto tablesDto) => await _tablesEventsRepository.AssignTablesAsync(tablesDto);

        public async Task<ActionResponse<GenerateTablesDto>> GenerateTablesAsync(GenerateTablesDto generateTablesDto) => await _tablesEventsRepository.GenerateTablesAsync(generateTablesDto);

        public async Task<ActionResponse<bool>> DeleteTablesAsync(int id) => await _tablesEventsRepository.DeleteTablesAsync(id);

        public async Task<ActionResponse<bool>> DeleteInvitatonFromTablesAsync(int id) => await _tablesEventsRepository.DeleteInvitatonFromTablesAsync(id);

        public async Task<ActionResponse<IEnumerable<TablesEvents>>> GetTablesByCodeAsync(string code) => await _tablesEventsRepository.GetTablesByCodeAsync(code);
    }
}