using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Shared;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.HistoryMessage
{
    public partial class HistoryMessagesIndex
    {
        private int totalPages;
        private int currentPage = 1;

        [Parameter, SupplyParameterFromQuery]
        public string? Page { get; set; }

        [Parameter, SupplyParameterFromQuery] public int? SelectedPageSize { get; set; } = 25;

        [Parameter, SupplyParameterFromQuery]
        public int? RecordsNumber { get; set; }

        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;

        [Parameter]
        public string? Id { get; set; }

        [Inject]
        private IRepository Repository { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private SweetAlertService SweetAlertService { get; set; } = default!;

        public List<HistoryMessages> HistoryMessages { get; set; } = new();

        protected override async Task OnParametersSetAsync()
        {
            // Validar RecordsNumber
            RecordsNumber = RecordsNumber is null or <= 0 ? 15 : RecordsNumber;

            // Validar Page desde Query
            if (!string.IsNullOrWhiteSpace(Page) && int.TryParse(Page, out var pageFromQuery))
            {
                currentPage = pageFromQuery <= 0 ? 1 : pageFromQuery;
            }
            else
            {
                currentPage = 1;
            }

            await LoadHistoryMessages(currentPage);
        }

        private async Task SelectedPage(int page)
        {
            currentPage = page;
            await LoadHistoryMessages(currentPage);
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadHistoryMessages(page);
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            await LoadHistoryMessages(1);
        }

        private async Task LoadHistoryMessages(int page)
        {
            var ok = await LoadListAsync(page);

            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private async Task LoadPagesAsync()
        {
            try
            {
                var url = $"api/messages/totalRecordAsync?RecordsNumber={RecordsNumber}";

                if (SelectedPageSize != null)
                {
                    url += $"&PageSize={SelectedPageSize}";
                }
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    url += $"&Filter={Filter}";
                }
                var responseHttp = await Repository.GetAsync<int>(url);

                if (responseHttp.Error)
                {
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return;
                }

                // El backend ya devuelve el total de páginas
                totalPages = responseHttp.Response;
            }
            catch (Exception ex)
            {
                await SweetAlertService.FireAsync("Error", ex.Message, SweetAlertIcon.Error);
            }
        }

        private async Task<bool> LoadListAsync(int page)
        {
            try
            {
                var url = $"api/messages/HistoryMessages/paginated?PageNumber={page}";
                if (SelectedPageSize != null)
                {
                    url += $"&PageSize={SelectedPageSize}";
                }
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    url += $"&Filter={Filter}";
                }
                var responseHttp = await Repository.GetAsync<List<HistoryMessages>>(url);

                if (responseHttp.Error)
                {
                    var message = await responseHttp.GetErrorMessageAsync();

                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);

                    if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        NavigationManager.NavigateTo("/events");
                    }

                    return false;
                }

                HistoryMessages = responseHttp.Response ?? new();
                return true;
            }
            catch (Exception ex)
            {
                await SweetAlertService.FireAsync("Error", ex.Message, SweetAlertIcon.Error);
                return false;
            }
        }
    }
}