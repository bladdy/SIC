using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.Net;

namespace SIC.Frontend.Pages.EventRequirements
{
    [Authorize(Roles = "Admin")]
    public partial class EventResponsesIndex
    {
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }

        private List<Event>? Events;

        private string? CopyingEventCode;
        private int currentPage = 1;
        private int totalPages;
        private int? DesignStatusFilter;

        protected override async Task OnInitializedAsync()
        {
            RecordsNumber ??= 15;
            if (!string.IsNullOrWhiteSpace(Page) && int.TryParse(Page, out var pageFromQuery))
            {
                currentPage = pageFromQuery;
            }
            await LoadEvents(currentPage);
        }

        private async Task LoadEvents(int page = 1)
        {
            var ok = await LoadListAsync(page);
            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/Events/activeResponsesTotal?PageSize={RecordsNumber ?? 15}";

            if (DesignStatusFilter.HasValue)
            {
                url += $"&DesignStatus={DesignStatusFilter.Value}";
            }

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&Filter={Filter}";
            }

            var responseHttp = await repository.GetAsync<int>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            totalPages = responseHttp.Response;
        }

        private async Task<bool> LoadListAsync(int page)
        {
            var url = $"api/Events/activeResponses?PageNumber={page}&PageSize={RecordsNumber ?? 15}";
            if (DesignStatusFilter.HasValue)
            {
                url += $"&DesignStatus={DesignStatusFilter.Value}";
            }
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&Filter={Filter}";
            }

            var responseHttp = await repository.GetAsync<List<Event>>(url);

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/events");
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return false;
                }
            }

            Events = responseHttp?.Response ?? new List<Event>();
            return true;
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadEvents(page);
        }

        private async Task OnDesignStatusFilterChangedAsync(ChangeEventArgs e)
        {
            DesignStatusFilter = int.TryParse(e.Value?.ToString(), out var value) ? value : null;
            await LoadEvents(1);
        }

        private async Task ApplyFilterAsync()
        {
            await LoadEvents(1);
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            DesignStatusFilter = null;
            await LoadEvents(1);
        }

        private static string GetDesignBadgeClass(DesignStatus status)
        {
            return status switch
            {
                DesignStatus.Completed => "sic-badge-success",
                DesignStatus.InProgress => "sic-badge-warning",
                _ => "sic-badge-gray"
            };
        }

        private async Task CopiarEventUrl(string Code)
        {
            CopyingEventCode = Code;
            var url = $"{NavigationManager.BaseUri}event/{Code}/requisitos";

            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", url);
            await Task.Delay(1500);
            CopyingEventCode = null;
        }
    }
}