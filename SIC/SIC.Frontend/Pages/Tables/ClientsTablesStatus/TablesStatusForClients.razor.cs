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
        private List<TablesEvents>? Tables;
        private int totalTables;
        private int totalSeats;
        private int occupiedSeats;
        private int availableSeats;

        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadTablesEventsAsync();
            ComputeStats();
        }

        private void ComputeStats()
        {
            totalTables = Tables?.Count ?? 0;
            totalSeats = Tables?.Sum(s => s.Seats) ?? 0;
            occupiedSeats = Tables?.Sum(s => s.OccupiedSeats) ?? 0;
            availableSeats = totalSeats - occupiedSeats;
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

        private string GetTableStatusClass(TablesEvents table)
        {
            if (table.Seats == 0) return "bg-secondary";
            double percentage = (double)table.OccupiedSeats / table.Seats;
            if (percentage > 1.0) return "bg-danger";
            if (percentage >= 0.9) return "bg-warning text-dark";
            if (percentage >= 0.5) return "bg-info";
            if (percentage > 0) return "bg-primary";
            return "bg-success";
        }

        private string GetTableStatusLabel(TablesEvents table)
        {
            if (table.Seats == 0) return "Sin lugares";
            double percentage = (double)table.OccupiedSeats / table.Seats;
            if (percentage > 1.0) return "Sobre capacidad";
            if (percentage >= 0.9) return "Casi llena";
            if (percentage >= 0.5) return "Disponible";
            if (percentage > 0) return "En uso";
            return "Libre";
        }
    }
}