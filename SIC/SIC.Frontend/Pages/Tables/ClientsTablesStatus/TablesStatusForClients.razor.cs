using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.Tables.ClientsTablesStatus
{
    public partial class TablesStatusForClients
    {
        [Parameter] public string? Code { get; set; }
        private int currentPage = 1;
        private int totalPages = 2;
        private List<TablesEvents> Tables = new();

        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadTablesEventsAsync();
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
        }

        private async Task LoadTablesEventsAsync()
        {
            var result = await Repository.GetAsync<List<TablesEvents>>($"api/Tables/tablesbycode/{Code}");
            if (result.Error)
            {
                var message = await result.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            Tables = result.Response ?? new List<TablesEvents>();
        }
    }
}